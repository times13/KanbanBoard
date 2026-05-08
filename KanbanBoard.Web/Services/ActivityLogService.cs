using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KanbanBoard.Web.Services;

public class ActivityLogService
{
    private readonly IActivityLogDA _logDA;
    private readonly IHubContext<KanbanHub> _hub;

    public ActivityLogService(IActivityLogDA logDA, IHubContext<KanbanHub> hub)
    {
        _logDA = logDA;
        _hub = hub;
    }

    /// <summary>
    /// Persiste l'action + broadcast SignalR aux membres du board pour rafraîchir leur panneau d'activité.
    /// </summary>
    public async Task LogAsync(
        int boardId,
        int userId,
        string entityType,
        int? entityId,
        string action,
        string? details = null)
    {
        await _logDA.LogAsync(boardId, userId, entityType, entityId, action, details);

        // Broadcast pour rafraîchir le panneau d'activité de tous les membres
        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId))
            .SendAsync("ActivityLogged", new { boardId = boardId });
    }
}