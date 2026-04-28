using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface ICardDA
{
    Task<int> CreateCardAsync(int columnId, string title, string? description, int createdByUserId);
    Task<KanbanCardViewModel?> GetCardAsync(int cardId);
    Task<bool> UpdateCardAsync(int cardId, string title, string? description, string priority, DateTime? dueDate, int? assigneeId);
    Task<bool> DeleteCardAsync(int cardId);

    /// <summary>
    /// Déplace une carte vers une autre colonne (ou réordonne dans la même).
    /// Utile pour le drag & drop du Jour 3.
    /// </summary>
    Task<bool> MoveCardAsync(int cardId, int targetColumnId, int newPosition);
}