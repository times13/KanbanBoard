using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.Results;
using KanbanBoard.LibrairieMetier.ViewModels;
using KanbanBoard.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KanbanBoard.Web.Controllers;

[Authorize]   // <-- Toutes les actions exigent une authentification
public class BoardController : Controller
{
    private readonly IBoardDA _boardDA;
    private readonly IHubContext<KanbanHub> _hub;

    public BoardController(IBoardDA boardDA, IHubContext<KanbanHub> hub)
    {
        _boardDA = boardDA;
        _hub = hub;
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

    // ---------- ADD MEMBER ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(AddMemberViewModel model)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserIsAdminAsync(model.BoardId, userId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Email ou rôle invalide.";
            return RedirectToAction(nameof(Details), new { id = model.BoardId });
        }

        var result = await _boardDA.AddMemberByEmailAsync(
            model.BoardId,
            model.Email,
            model.Role);

        switch (result)
        {
            case AddMemberResult.Success:
                TempData["SuccessMessage"] = $"Utilisateur {model.Email} ajouté en tant que {model.Role}.";
                await _hub.Clients
                    .Group(KanbanHub.BoardGroupName(model.BoardId))
                    .SendAsync("BoardChanged", new
                    {
                        action = "MemberAdded",
                        email = model.Email,
                        role = model.Role,
                        triggeredBy = User.Identity?.Name
                    });
                break;

            case AddMemberResult.UserNotFound:
                TempData["ErrorMessage"] = $"Aucun utilisateur trouvé avec l'email « {model.Email} ». L'utilisateur doit d'abord créer un compte.";
                break;

            case AddMemberResult.AlreadyMember:
                TempData["ErrorMessage"] = "Cet utilisateur est déjà membre du tableau.";
                break;

            case AddMemberResult.InvalidRole:
                TempData["ErrorMessage"] = "Rôle invalide.";
                break;
        }

        return RedirectToAction(nameof(Details), new { id = model.BoardId });
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