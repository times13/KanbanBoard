namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface ICardReadDA
{
    /// <summary>
    /// Marque une carte comme "lue" par un utilisateur (upsert dans CARD_READ).
    /// Si la ligne existe → on met à jour LastReadAt à maintenant.
    /// Sinon → on crée la ligne.
    /// </summary>
    Task MarkAsReadAsync(int userId, int cardId);

    /// <summary>
    /// Pour un board et un utilisateur, retourne un dictionnaire {cardId → unreadCount}
    /// où unreadCount = nombre de commentaires postés APRÈS LastReadAt
    /// (ou tous les commentaires si la ligne n'existe pas).
    /// Les commentaires écrits par l'utilisateur lui-même ne sont PAS comptés comme "non lus".
    /// </summary>
    Task<Dictionary<int, int>> GetUnreadCountsForBoardAsync(int boardId, int userId);
}