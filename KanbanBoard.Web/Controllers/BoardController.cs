using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.Results;
using KanbanBoard.LibrairieMetier.ViewModels;
using KanbanBoard.Web.Hubs;
using KanbanBoard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KanbanBoard.Web.Controllers;

[Authorize]   // <-- Toutes les actions exigent une authentification
public class BoardController : Controller
{
    private readonly IBoardDA _boardDA;
    private readonly IUserDA _userDA;
    private readonly IHubContext<KanbanHub> _hub;
    private readonly NotificationService _notif;

    public BoardController(IBoardDA boardDA, IUserDA userDA, IHubContext<KanbanHub> hub,
        NotificationService notif)
    {
        _boardDA = boardDA;
        _userDA = userDA;
        _hub = hub;
        _notif = notif;
        
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

                // Récupère l'Id du user invité pour notifier
                var invitedUserId = await GetUserIdByEmail(model.Email);
                var boardTitle = await _boardDA.GetBoardTitleAsync(model.BoardId) ?? "(sans titre)";

                if (invitedUserId.HasValue)
                {
                    await _notif.NotifyUserAsync(
                        userId: invitedUserId.Value,
                        actorId: userId,
                        type: "MemberAdded",
                        message: $"{User.Identity?.Name} vous a invité au tableau « {boardTitle} » en tant que {model.Role}",
                        boardId: model.BoardId);
                }

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

    // ---------- CHANGE MEMBER ROLE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeMemberRole(int boardId, int targetUserId, string newRole)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserIsAdminAsync(boardId, userId))
            return Forbid();

        var result = await _boardDA.ChangeMemberRoleAsync(boardId, targetUserId, newRole);

        switch (result)
        {
            case ChangeRoleResult.Success:
                TempData["SuccessMessage"] = $"Rôle modifié en {newRole}.";
                var boardTitleR = await _boardDA.GetBoardTitleAsync(boardId) ?? "(sans titre)";
                await _notif.NotifyUserAsync(
                    userId: targetUserId,
                    actorId: userId,
                    type: "MemberRoleChanged",
                    message: $"{User.Identity?.Name} a changé votre rôle en {newRole} sur le tableau « {boardTitleR} »",
                    boardId: boardId);

                await _hub.Clients
                    .Group(KanbanHub.BoardGroupName(boardId))
                    .SendAsync("BoardChanged", new
                    {
                        action = "MemberRoleChanged",
                        targetUserId = targetUserId,
                        newRole = newRole,
                        triggeredBy = User.Identity?.Name
                    });
                break;

            case ChangeRoleResult.CannotChangeOwnerRole:
                TempData["ErrorMessage"] = "Le rôle du propriétaire du tableau ne peut pas être modifié.";
                break;

            case ChangeRoleResult.MemberNotFound:
                TempData["ErrorMessage"] = "Membre introuvable.";
                break;

            case ChangeRoleResult.InvalidRole:
                TempData["ErrorMessage"] = "Rôle invalide.";
                break;
        }

        return RedirectToAction(nameof(Details), new { id = boardId });
    }

    // ---------- REMOVE MEMBER ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int boardId, int targetUserId)
    {
        var userId = GetCurrentUserId();

        if (!await _boardDA.UserIsAdminAsync(boardId, userId))
            return Forbid();

        var result = await _boardDA.RemoveMemberAsync(boardId, targetUserId);

        switch (result)
        {
            case RemoveMemberResult.Success:
                TempData["SuccessMessage"] = "Membre retiré du tableau.";

                var boardTitleX = await _boardDA.GetBoardTitleAsync(boardId) ?? "(sans titre)";
                await _notif.NotifyUserAsync(
                    userId: targetUserId,
                    actorId: userId,
                    type: "MemberRemoved",
                    message: $"{User.Identity?.Name} vous a retiré du tableau « {boardTitleX} »",
                    boardId: boardId);

                await _hub.Clients
                    .Group(KanbanHub.BoardGroupName(boardId))
                    .SendAsync("BoardChanged", new
                    {
                        action = "MemberRemoved",
                        targetUserId = targetUserId,
                        triggeredBy = User.Identity?.Name
                    });
                break;

            case RemoveMemberResult.CannotRemoveOwner:
                TempData["ErrorMessage"] = "Le propriétaire du tableau ne peut pas être retiré.";
                break;

            case RemoveMemberResult.MemberNotFound:
                TempData["ErrorMessage"] = "Membre introuvable.";
                break;
        }

        return RedirectToAction(nameof(Details), new { id = boardId });
    }

    // ---------- LEAVE BOARD ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveBoard(int boardId)
    {
        var userId = GetCurrentUserId();

        var result = await _boardDA.LeaveBoardAsync(boardId, userId);

        switch (result)
        {
            case LeaveBoardResult.Success:
                TempData["SuccessMessage"] = "Vous avez quitté le tableau.";
                await _hub.Clients
                    .Group(KanbanHub.BoardGroupName(boardId))
                    .SendAsync("BoardChanged", new
                    {
                        action = "MemberLeft",
                        targetUserId = userId,
                        triggeredBy = User.Identity?.Name
                    });
                return RedirectToAction(nameof(MyBoards));

            case LeaveBoardResult.OwnerCannotLeave:
                TempData["ErrorMessage"] = "Le propriétaire ne peut pas quitter le tableau. Supprimez-le si vous ne le voulez plus.";
                break;

            case LeaveBoardResult.NotAMember:
                TempData["ErrorMessage"] = "Vous n'êtes pas membre de ce tableau.";
                break;
        }

        return RedirectToAction(nameof(Details), new { id = boardId });
    }

    // ---------- HELPERS ----------

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }

    private async Task<int?> GetUserIdByEmail(string email)
    {
        // Cherche l'Id du user à partir de son email — utilise IBoardDA pour pas dupliquer
        var members = await _boardDA.GetMembersAsync(0); // hack temporaire — voir note
                                                         // ... 
        return null; // placeholder
    }
}