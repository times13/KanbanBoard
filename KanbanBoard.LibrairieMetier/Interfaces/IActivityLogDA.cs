using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IActivityLogDA
{
    /// <summary>
    /// Enregistre une action dans le log + purge automatique si > 100 entrées pour ce board.
    /// </summary>
    Task LogAsync(int boardId, int userId, string entityType, int? entityId, string action, string? details = null);

    /// <summary>
    /// Liste les N actions les plus récentes d'un board avec username de l'auteur.
    /// </summary>
    Task<List<ActivityLogItemViewModel>> GetForBoardAsync(int boardId, int limit = 20);
}