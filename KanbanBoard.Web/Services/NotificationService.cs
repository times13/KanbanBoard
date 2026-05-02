using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KanbanBoard.Web.Services;

public class NotificationService
{
    private readonly INotificationDA _notifDA;
    private readonly IHubContext<KanbanHub> _hub;

    public NotificationService(INotificationDA notifDA, IHubContext<KanbanHub> hub)
    {
        _notifDA = notifDA;
        _hub = hub;
    }

    /// <summary>
    /// Crée une notification + la broadcaste en temps réel au destinataire via SignalR.
    /// </summary>
    public async Task NotifyUserAsync(
        int userId,
        int actorId,
        string type,
        string message,
        int? boardId = null,
        int? cardId = null)
    {
        // 1. Persister
        var notifId = await _notifDA.CreateAsync(userId, actorId, type, message, boardId, cardId);

        // 2. Broadcast au groupe personnel du destinataire
        await _hub.Clients
            .Group(KanbanHub.UserGroupName(userId))
            .SendAsync("NotificationReceived", new
            {
                id = notifId,
                type = type,
                message = message,
                boardId = boardId,
                cardId = cardId,
                createdAt = DateTime.UtcNow
            });
    }
}