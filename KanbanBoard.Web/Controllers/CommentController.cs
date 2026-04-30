using System.Security.Claims;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class CommentController : Controller
{
    private readonly ICommentDA _commentDA;
    private readonly ICardDA _cardDA;
    private readonly IBoardDA _boardDA;
    private readonly IHubContext<KanbanHub> _hub;

    public CommentController(
        ICommentDA commentDA,
        ICardDA cardDA,
        IBoardDA boardDA,
        IHubContext<KanbanHub> hub)
    {
        _commentDA = commentDA;
        _cardDA = cardDA;
        _boardDA = boardDA;
        _hub = hub;
    }

    // ---------- ADD ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int cardId, int boardId, string content)
    {
        var userId = GetCurrentUserId();

        // Tous les membres (sauf Viewer) peuvent commenter
        if (!await _boardDA.UserHasAccessAsync(boardId, userId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Le commentaire ne peut pas être vide.";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        if (content.Length > 2000)
        {
            TempData["ErrorMessage"] = "Le commentaire est trop long (2000 caractères max).";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        await _commentDA.AddCommentAsync(cardId, userId, content);

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId))
            .SendAsync("BoardChanged", new
            {
                action = "CommentAdded",
                cardId = cardId,
                triggeredBy = User.Identity?.Name
            });

        TempData["SuccessMessage"] = "Commentaire ajouté.";
        return RedirectToAction("Edit", "Card", new { id = cardId });
    }

    // ---------- DELETE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var comment = await _commentDA.GetCommentAsync(id);
        if (comment == null) return NotFound();

        var boardId = await _commentDA.GetCommentBoardIdAsync(id);
        if (boardId == null) return NotFound();

        // L'auteur OU un Admin peut supprimer
        var isAuthor = comment.AuthorId == userId;
        var isAdmin = await _boardDA.UserIsAdminAsync(boardId.Value, userId);

        if (!isAuthor && !isAdmin)
            return Forbid();

        await _commentDA.DeleteCommentAsync(id);

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId.Value))
            .SendAsync("BoardChanged", new
            {
                action = "CommentDeleted",
                cardId = comment.CardId,
                triggeredBy = User.Identity?.Name
            });

        TempData["SuccessMessage"] = "Commentaire supprimé.";
        return RedirectToAction("Edit", "Card", new { id = comment.CardId });
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