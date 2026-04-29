using System.Security.Claims;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using KanbanBoard.Web.Hubs;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class CardController : Controller
{
    private readonly ICardDA _cardDA;
    private readonly IBoardDA _boardDA;
    private readonly IHubContext<KanbanHub> _hub;

    public CardController(ICardDA cardDA, IBoardDA boardDA, IHubContext<KanbanHub> hub)
    {
        _cardDA = cardDA;
        _boardDA = boardDA;
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

        var model = new EditCardViewModel
        {
            Id = card.Id,
            BoardId = boardId.Value,
            Title = card.Title,
            Description = card.Description,
            Priority = card.Priority,
            DueDate = card.DueDate
        };
        ViewData["IsAdmin"] = await _boardDA.UserIsAdminAsync(boardId.Value, userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCardViewModel model)
    {
        var userId = GetCurrentUserId();
        var hasAccess = await _boardDA.UserHasAccessAsync(model.BoardId, userId);
        if (!hasAccess) return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        var success = await _cardDA.UpdateCardAsync(
            model.Id,
            model.Title,
            model.Description,
            model.Priority,
            model.DueDate,
            null   // assigneeId sera géré plus tard
        );

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

    // ---------- HELPER ----------

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}