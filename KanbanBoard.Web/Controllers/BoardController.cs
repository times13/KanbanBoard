using System.Security.Claims;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanBoard.Web.Controllers;

[Authorize]   // <-- Toutes les actions exigent une authentification
public class BoardController : Controller
{
    private readonly IBoardDA _boardDA;

    public BoardController(IBoardDA boardDA)
    {
        _boardDA = boardDA;
    }

    // ---------- MES BOARDS ----------

    [HttpGet]
    public async Task<IActionResult> MyBoards()
    {
        var userId = GetCurrentUserId();
        var boards = await _boardDA.GetBoardsForUserAsync(userId);
        return View(boards);
    }

    // ---------- CREATE ----------

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBoardViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBoardViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        var newBoardId = await _boardDA.CreateBoardAsync(userId, model.Title, model.Description);

        TempData["SuccessMessage"] = $"Tableau « {model.Title} » créé avec succès.";
        return RedirectToAction(nameof(Details), new { id = newBoardId });
    }

    // ---------- DETAILS (le Kanban lui-même) ----------

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        var board = await _boardDA.GetBoardDetailsAsync(id, userId);

        if (board == null)
            return NotFound();   // soit le board n'existe pas, soit l'utilisateur n'y a pas accès

        return View(board);
    }

    // ---------- HELPERS ----------

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}