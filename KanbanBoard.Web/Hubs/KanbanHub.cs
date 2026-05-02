using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KanbanBoard.Web.Hubs;

[Authorize]
public class KanbanHub : Hub
{
    /// <summary>
    /// Appelé par le client pour rejoindre le "groupe" d'un tableau.
    /// Tous les utilisateurs sur ce tableau seront notifiés ensemble.
    /// </summary>
    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, BoardGroupName(boardId));
    }

    /// <summary>
    /// Appelé par le client quand il quitte la page (optionnel).
    /// </summary>
    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BoardGroupName(boardId));
    }

    /// <summary>
    /// Helper pour standardiser le nom des groupes.
    /// </summary>
    public static string BoardGroupName(int boardId) => $"board-{boardId}";

    public static string UserGroupName(int userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        // À la connexion, le user rejoint automatiquement son propre groupe
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(userId));
        }
        await base.OnConnectedAsync();
    }

}