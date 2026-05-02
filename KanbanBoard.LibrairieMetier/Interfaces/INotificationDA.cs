using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface INotificationDA
{
    /// <summary>Crée une notification pour un utilisateur destinataire et retourne son Id.</summary>
    Task<int> CreateAsync(
        int userId,
        int actorId,
        string type,
        string message,
        int? boardId = null,
        int? cardId = null);

    /// <summary>Liste les N notifications les plus récentes d'un utilisateur (pour le dropdown).</summary>
    Task<List<NotificationItemViewModel>> GetRecentForUserAsync(int userId, int limit = 10);

    /// <summary>Compte les notifications non lues d'un utilisateur (pour le badge).</summary>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>Marque une notification comme lue (n'agit que si elle appartient au user).</summary>
    Task<bool> MarkAsReadAsync(int notificationId, int userId);

    /// <summary>Marque toutes les notifications d'un user comme lues.</summary>
    Task<int> MarkAllAsReadAsync(int userId);
}