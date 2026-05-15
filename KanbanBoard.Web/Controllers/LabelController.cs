using KanbanBoard.LibrairieMetier.Constants;
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
public class LabelController : Controller
{
    private readonly ILabelDA _labelDA;
    private readonly IBoardDA _boardDA;
    private readonly ICardDA _cardDA;
    private readonly IHubContext<KanbanHub> _hub;
    private readonly ActivityLogService _activityLog;

    public LabelController(
        ILabelDA labelDA,
        IBoardDA boardDA,
        ICardDA cardDA,
        IHubContext<KanbanHub> hub,
        ActivityLogService activityLog)
    {
        _labelDA = labelDA;
        _boardDA = boardDA;
        _cardDA = cardDA;
        _hub = hub;
        _activityLog = activityLog;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---------- GET — Page de gestion des labels du board ----------

    [HttpGet]
    public async Task<IActionResult> ManageForBoard(int boardId)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserHasAccessAsync(boardId, userId))
        {
            TempData["ErrorMessage"] = "Vous n'avez pas accès à ce tableau.";
            return RedirectToAction("MyBoards", "Board");
        }

        var labels = await _labelDA.GetForBoardAsync(boardId);

        ViewData["BoardId"] = boardId;
        ViewData["IsAdmin"] = await _boardDA.UserIsAdminAsync(boardId, userId);

        return View(labels);
    }

    // ---------- POST — Créer un label (Admin) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int boardId, string name, string color)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserIsAdminAsync(boardId, userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            TempData["ErrorMessage"] = "Nom invalide (1 à 50 caractères).";
            return RedirectToAction(nameof(ManageForBoard), new { boardId });
        }

        if (string.IsNullOrWhiteSpace(color) || !System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
        {
            TempData["ErrorMessage"] = "Couleur invalide (format #RRGGBB attendu).";
            return RedirectToAction(nameof(ManageForBoard), new { boardId });
        }

        var labelId = await _labelDA.CreateAsync(boardId, name, color);

        await _activityLog.LogAsync(
            boardId: boardId,
            userId: userId,
            entityType: ActivityEntityType.Label,
            entityId: labelId,
            action: ActivityAction.LabelCreated,
            details: $"\"{name}\" ({color})");

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId))
            .SendAsync("BoardChanged", new
            {
                action = "LabelCreated",
                labelId,
                triggeredBy = User.Identity?.Name
            });

        TempData["SuccessMessage"] = $"Label « {name} » créé.";
        return RedirectToAction(nameof(ManageForBoard), new { boardId });
    }

    // ---------- POST — Modifier un label (Admin) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string color)
    {
        var userId = GetCurrentUserId();

        var boardId = await _labelDA.GetBoardIdAsync(id);
        if (boardId == null) return NotFound();

        if (!await _boardDA.UserIsAdminAsync(boardId.Value, userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            TempData["ErrorMessage"] = "Nom invalide.";
            return RedirectToAction(nameof(ManageForBoard), new { boardId });
        }

        if (string.IsNullOrWhiteSpace(color) || !System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$"))
        {
            TempData["ErrorMessage"] = "Couleur invalide.";
            return RedirectToAction(nameof(ManageForBoard), new { boardId });
        }

        var success = await _labelDA.UpdateAsync(id, name, color);

        if (success)
        {
            await _activityLog.LogAsync(
                boardId: boardId.Value,
                userId: userId,
                entityType: ActivityEntityType.Label,
                entityId: id,
                action: ActivityAction.LabelUpdated,
                details: $"\"{name}\" ({color})");

            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "LabelUpdated",
                    labelId = id,
                    triggeredBy = User.Identity?.Name
                });

            TempData["SuccessMessage"] = $"Label « {name} » mis à jour.";
        }
        else
        {
            TempData["ErrorMessage"] = "Label introuvable.";
        }

        return RedirectToAction(nameof(ManageForBoard), new { boardId });
    }

    // ---------- POST — Supprimer un label (Admin) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var boardId = await _labelDA.GetBoardIdAsync(id);
        if (boardId == null) return NotFound();

        if (!await _boardDA.UserIsAdminAsync(boardId.Value, userId))
            return Forbid();

        var label = await _labelDA.GetByIdAsync(id);
        var labelName = label?.Name ?? "?";

        var success = await _labelDA.DeleteAsync(id);

        if (success)
        {
            await _activityLog.LogAsync(
                boardId: boardId.Value,
                userId: userId,
                entityType: ActivityEntityType.Label,
                entityId: id,
                action: ActivityAction.LabelDeleted,
                details: $"\"{labelName}\"");

            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "LabelDeleted",
                    labelId = id,
                    triggeredBy = User.Identity?.Name
                });

            TempData["SuccessMessage"] = $"Label « {labelName} » supprimé.";
        }
        else
        {
            TempData["ErrorMessage"] = "Label introuvable.";
        }

        return RedirectToAction(nameof(ManageForBoard), new { boardId });
    }

    // ---------- POST — Assigner un label à une carte (Member+) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignToCard(int cardId, int labelId)
    {
        var userId = GetCurrentUserId();

        var boardId = await _cardDA.GetCardBoardIdAsync(cardId);
        if (boardId == null) return NotFound();

        if (!await _boardDA.UserCanWriteAsync(boardId.Value, userId))
            return Forbid();

        var labelBoardId = await _labelDA.GetBoardIdAsync(labelId);
        if (labelBoardId != boardId)
        {
            TempData["ErrorMessage"] = "Ce label n'appartient pas à ce tableau.";
            return RedirectToAction("Details", "Card", new { id = cardId });
        }

        var success = await _labelDA.AssignToCardAsync(cardId, labelId);

        if (success)
        {
            var label = await _labelDA.GetByIdAsync(labelId);
            var card = await _cardDA.GetCardAsync(cardId);

            await _activityLog.LogAsync(
                boardId: boardId.Value,
                userId: userId,
                entityType: ActivityEntityType.Label,
                entityId: labelId,
                action: ActivityAction.LabelAddedToCard,
                details: $"\"{label?.Name}\" sur \"{card?.Title}\"");

            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "LabelAddedToCard",
                    cardId,
                    labelId,
                    triggeredBy = User.Identity?.Name
                });
        }

        return RedirectToAction("Details", "Card", new { id = cardId });
    }

    // ---------- POST — Désassigner un label d'une carte (Member+) ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnassignFromCard(int cardId, int labelId)
    {
        var userId = GetCurrentUserId();

        var boardId = await _cardDA.GetCardBoardIdAsync(cardId);
        if (boardId == null) return NotFound();

        if (!await _boardDA.UserCanWriteAsync(boardId.Value, userId))
            return Forbid();

        var success = await _labelDA.UnassignFromCardAsync(cardId, labelId);

        if (success)
        {
            var label = await _labelDA.GetByIdAsync(labelId);
            var card = await _cardDA.GetCardAsync(cardId);

            await _activityLog.LogAsync(
                boardId: boardId.Value,
                userId: userId,
                entityType: ActivityEntityType.Label,
                entityId: labelId,
                action: ActivityAction.LabelRemovedFromCard,
                details: $"\"{label?.Name}\" de \"{card?.Title}\"");

            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "LabelRemovedFromCard",
                    cardId,
                    labelId,
                    triggeredBy = User.Identity?.Name
                });
        }

        return RedirectToAction("Details", "Card", new { id = cardId });
    }
}