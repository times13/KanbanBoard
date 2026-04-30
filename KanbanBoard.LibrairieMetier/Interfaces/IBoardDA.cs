using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IBoardDA
{
    /// <summary>
    /// Liste les boards accessibles par l'utilisateur (owner ou membre).
    /// </summary>
    Task<List<BoardListItemViewModel>> GetBoardsForUserAsync(int userId);

    /// <summary>
    /// Récupère le détail d'un board avec ses colonnes et cartes.
    /// Retourne null si le board n'existe pas ou si l'utilisateur n'y a pas accès.
    /// </summary>
    Task<KanbanBoardViewModel?> GetBoardDetailsAsync(int boardId, int userId);

    /// <summary>
    /// Crée un nouveau board avec 3 colonnes par défaut (À faire, En cours, Terminé).
    /// L'owner devient automatiquement membre Admin.
    /// Retourne l'Id du board créé.
    /// </summary>
    Task<int> CreateBoardAsync(int ownerId, string title, string? description);

    /// <summary>
    /// Vérifie si un utilisateur a accès à un board (owner ou membre).
    /// </summary>
    Task<bool> UserHasAccessAsync(int boardId, int userId);

    /// <summary>
    /// Vérifie si l'utilisateur est Admin du board (owner OU membre avec Role='Admin').
    /// </summary>
    Task<bool> UserIsAdminAsync(int boardId, int userId);

    /// <summary>Liste les membres d'un board (pour le dropdown assignee).</summary>
    Task<List<BoardMemberItemViewModel>> GetMembersAsync(int boardId);
}