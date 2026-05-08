using KanbanBoard.LibrairieMetier.Constants;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.Web.Hubs;
using KanbanBoard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class ColumnController : Controller
{
    private readonly IColumnDA _columnDA;
    private readonly IBoardDA _boardDA;
    private readonly ActivityLogService _activityLog;
    private readonly IHubContext<KanbanHub> _hub;

    public ColumnController(IColumnDA columnDA, IBoardDA boardDA, ActivityLogService activityLog, IHubContext<KanbanHub> hub)
    {
        _columnDA = columnDA;
        _boardDA = boardDA;
        _activityLog = activityLog;
        _hub = hub;
    }

    // ---------- CREATE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int boardId, string title)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserIsAdminAsync(boardId, userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 2)
        {
            TempData["ErrorMessage"] = "Le titre de la colonne doit faire au moins 2 caractères.";
            return RedirectToAction("Details", "Board", new { id = boardId });
        }

        if (title.Length > 100)
        {
            TempData["ErrorMessage"] = "Le titre de la colonne est trop long (100 caractères max).";
            return RedirectToAction("Details", "Board", new { id = boardId });
        }

        var newColumnId = await _columnDA.CreateColumnAsync(boardId, title);

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId))
            .SendAsync("BoardChanged", new
            {
                action = "ColumnCreated",
                columnId = newColumnId,
                title = title.Trim(),
                triggeredBy = User.Identity?.Name
            });

        TempData["SuccessMessage"] = $"Colonne « {title.Trim()} » ajoutée.";

        await _activityLog.LogAsync(
            boardId: boardId,
            userId: userId,
            entityType: ActivityEntityType.Column,
            entityId: newColumnId,
            action: ActivityAction.ColumnCreated,
            details: $"\"{title}\"");
        return RedirectToAction("Details", "Board", new { id = boardId });
    }

    // ---------- RENAME ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(int id, string newTitle)
    {
        var boardId = await _columnDA.GetColumnBoardIdAsync(id);
        if (boardId == null) return NotFound();

        var userId = GetCurrentUserId();
        if (!await _boardDA.UserIsAdminAsync(boardId.Value, userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(newTitle) || newTitle.Trim().Length < 2)
        {
            TempData["ErrorMessage"] = "Le nouveau titre doit faire au moins 2 caractères.";
            return RedirectToAction("Details", "Board", new { id = boardId });
        }

        // AVANT le rename : récupérer l'ancien titre
        var oldTitle = await _columnDA.GetColumnTitleAsync(id) ?? "?";

        var success = await _columnDA.RenameColumnAsync(id, newTitle);

        if (success)
        {
            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "ColumnRenamed",
                    columnId = id,
                    title = newTitle.Trim(),
                    triggeredBy = User.Identity?.Name
                });

            TempData["SuccessMessage"] = "Colonne renommée.";
        }
        else
        {
            TempData["ErrorMessage"] = "Colonne introuvable.";
        }

        await _activityLog.LogAsync(
            boardId: boardId.Value, // le BoardId récupéré au début
            userId: userId,
            entityType: ActivityEntityType.Column,
            entityId: id,
            action: ActivityAction.ColumnRenamed,
            details: $"\"{oldTitle}\" en \"{newTitle}\"");
        return RedirectToAction("Details", "Board", new { id = boardId });
    }

    // ---------- DELETE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var boardId = await _columnDA.GetColumnBoardIdAsync(id);
        if (boardId == null) return NotFound();

        var userId = GetCurrentUserId();
        if (!await _boardDA.UserIsAdminAsync(boardId.Value, userId))
            return Forbid();
        // AVANT suppression : récupérer le titre + nombre de cartes
        var oldTitle = await _columnDA.GetColumnTitleAsync(id) ?? "?";

        var success = await _columnDA.DeleteColumnAsync(id);

        if (success)
        {
            await _hub.Clients
                .Group(KanbanHub.BoardGroupName(boardId.Value))
                .SendAsync("BoardChanged", new
                {
                    action = "ColumnDeleted",
                    columnId = id,
                    triggeredBy = User.Identity?.Name
                });

            TempData["SuccessMessage"] = "Colonne supprimée.";
            await _activityLog.LogAsync(
                boardId: boardId.Value,
                userId: userId,
                entityType: ActivityEntityType.Column,
                entityId: id,
                action: ActivityAction.ColumnDeleted,
                details: $"\"{oldTitle}\"");
        }
        else
        {
            TempData["ErrorMessage"] = "Colonne introuvable.";
        }

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