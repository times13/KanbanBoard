using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using KanbanBoard.Web.Hubs;
using KanbanBoard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class CardController : Controller
{
    private readonly ICardDA _cardDA;
    private readonly IBoardDA _boardDA;
    private readonly ICommentDA _commentDA;
    private readonly ICardReadDA _cardReadDA;
    private readonly NotificationService _notif;
    private readonly IHubContext<KanbanHub> _hub;

    public CardController(ICardDA cardDA, IBoardDA boardDA, ICommentDA commentDA, ICardReadDA cardReadDA, NotificationService notif, IHubContext<KanbanHub> hub)
    {
        _cardDA = cardDA;
        _boardDA = boardDA;
        _commentDA = commentDA;
        _cardReadDA = cardReadDA;
        _notif = notif;
        _hub = hub;
    }

    // ---------- CREATE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCardViewModel model)
    {
        var userId = GetCurrentUserId();

        // Seul un Admin peut créer une carte
        if (!await _boardDA.UserIsAdminAsync(model.BoardId, userId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Le titre de la carte est requis (2 caractères minimum).";
            return RedirectToAction("Details", "Board", new { id = model.BoardId });
        }

        var newCardId = await _cardDA.CreateCardAsync(model.ColumnId, model.Title, model.Description, userId);

        // 🔔 Broadcaster à tous les utilisateurs connectés sur ce tableau
        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(model.BoardId))
            .SendAsync("BoardChanged", new
            {
                action = "CardCreated",
                cardId = newCardId,
                columnId = model.ColumnId,
                title = model.Title,
                triggeredBy = User.Identity?.Name
            });

        TempData["SuccessMessage"] = $"Carte « {model.Title} » créée.";
        return RedirectToAction("Details", "Board", new { id = model.BoardId });
    }

    // ---------- EDIT ----------

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var card = await _cardDA.GetCardAsync(id);
        if (card == null) return NotFound();

        var boardId = await _cardDA.GetCardBoardIdAsync(id);
        if (boardId == null) return NotFound();

        var userId = GetCurrentUserId();
        var hasAccess = await _boardDA.UserHasAccessAsync(boardId.Value, userId);
        if (!hasAccess) return Forbid();

        // Marquer la carte comme "lue" par l'utilisateur courant (pour les non-lus)
        await _cardReadDA.MarkAsReadAsync(userId, id);

        var members = await _boardDA.GetMembersAsync(boardId.Value);
        var comments = await _commentDA.GetForCardAsync(id);

        var model = new EditCardViewModel
        {
            Id = card.Id,
            BoardId = boardId.Value,
            Title = card.Title,
            Description = card.Description,
            Priority = card.Priority,
            DueDate = card.DueDate,
            AssigneeId = card.AssigneeId,
            AvailableMembers = members,
            CurrentAssigneeUsername = card.AssigneeUsername,
            Comments = comments
        };

        ViewData["IsAdmin"] = await _boardDA.UserIsAdminAsync(boardId.Value, userId);
        ViewData["CurrentUserId"] = userId;
        ViewData["CanWrite"] = await _boardDA.UserCanWriteAsync(boardId.Value, userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCardViewModel model)
    {
        var userId = GetCurrentUserId();
        if (!await _boardDA.UserCanWriteAsync(model.BoardId, userId))
        {
            TempData["ErrorMessage"] = "Vous êtes en lecture seule sur ce tableau, vous ne pouvez pas modifier les cartes.";
            return RedirectToAction("Details", "Board", new { id = model.BoardId });
        }

        if (!ModelState.IsValid)
        {
            model.AvailableMembers = await _boardDA.GetMembersAsync(model.BoardId);
            model.Comments = await _commentDA.GetForCardAsync(model.Id);
            return View(model);
        }

        // Récupérer l'ancien assignee pour détecter le changement
        var oldCard = await _cardDA.GetCardAsync(model.Id);
        var oldAssigneeId = oldCard?.AssigneeId;

        var success = await _cardDA.UpdateCardAsync(
            model.Id,
            model.Title,
            model.Description,
            model.Priority,
            model.DueDate,
            model.AssigneeId    // assigneeId sera géré plus tard
        );

        // Si l'assignee a changé et qu'il y en a un nouveau (pas null), on notifie
        if (success && model.AssigneeId.HasValue && model.AssigneeId != oldAssigneeId
            && model.AssigneeId.Value != userId) // ne pas se notifier soi-même
        {
            var boardTitle = await _boardDA.GetBoardTitleAsync(model.BoardId) ?? "(sans titre)";
            await _notif.NotifyUserAsync(
                userId: model.AssigneeId.Value,
                actorId: userId,
                type: "CardAssigned",
                message: $"{User.Identity?.Name} vous a assigné à la carte « {model.Title} » du tableau « {boardTitle} »",
                boardId: model.BoardId,
                cardId: model.Id);
        }

        if (!success)
        {
            TempData["ErrorMessage"] = "Carte introuvable.";
            return RedirectToAction("Details", "Board", new { id = model.BoardId });
        }

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(model.BoardId))
            .SendAsync("BoardChanged", new
            {
                action = "CardUpdated",
                cardId = model.Id,
                title = model.Title,
                triggeredBy = User.Identity?.Name
            });
        TempData["SuccessMessage"] = "Carte mise à jour.";
        return RedirectToAction("Details", "Board", new { id = model.BoardId });
    }

    // ---------- DELETE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int boardId)
    {
        var userId = GetCurrentUserId();
        if (!await _boardDA.UserIsAdminAsync(boardId, userId))
            return Forbid();

        var success = await _cardDA.DeleteCardAsync(id);

        if (success)
        {
            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId))
                .SendAsync("BoardChanged", new
                {
                    action = "CardDeleted",
                    cardId = id,
                    triggeredBy = User.Identity?.Name
                });
            TempData["SuccessMessage"] = "Carte supprimée.";
        }
        else
            TempData["ErrorMessage"] = "Carte introuvable.";

        return RedirectToAction("Details", "Board", new { id = boardId });
    }

    // ---------- MOVE (drag & drop) ----------

    public class MoveCardRequest
    {
        public int CardId { get; set; }
        public int TargetColumnId { get; set; }
        public int NewPosition { get; set; }
        public int BoardId { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move([FromBody] MoveCardRequest request)
    {
        var userId = GetCurrentUserId();

        // Membre ou Admin peut déplacer (Viewer non — UserHasAccess + role check)
        if (!await _boardDA.UserCanWriteAsync(request.BoardId, userId))
            return Json(new { success = false, error = "Vous êtes en lecture seule." });

        var success = await _cardDA.MoveCardAsync(request.CardId, request.TargetColumnId, request.NewPosition);
        if (!success)
            return NotFound();

        // Broadcast aux autres clients
        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(request.BoardId))
            .SendAsync("CardMoved", new
            {
                cardId = request.CardId,
                targetColumnId = request.TargetColumnId,
                newPosition = request.NewPosition,
                triggeredBy = User.Identity?.Name
            });

        return Ok(new { success = true });
    }

    // ---------- HELPER ----------

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}