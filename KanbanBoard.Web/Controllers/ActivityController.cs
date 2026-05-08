using System.Security.Claims;
using KanbanBoard.LibrairieMetier.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class ActivityController : Controller
{
    private readonly IActivityLogDA _logDA;
    private readonly IBoardDA _boardDA;

    public ActivityController(IActivityLogDA logDA, IBoardDA boardDA)
    {
        _logDA = logDA;
        _boardDA = boardDA;
    }

    // ---------- API : récupérer l'activité récente d'un board ----------

    [HttpGet]
    public async Task<IActionResult> GetRecent(int boardId, int limit = 20)
    {
        var userId = GetCurrentUserId();

        // Tout membre du board peut voir l'activité (même Viewer)
        if (!await _boardDA.UserHasAccessAsync(boardId, userId))
            return Forbid();

        var logs = await _logDA.GetForBoardAsync(boardId, limit);
        return Json(logs);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}