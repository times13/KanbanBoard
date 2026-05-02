using KanbanBoard.LibrairieMetier.Results;
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

    /// <summary>Ajoute un membre au board par son email.</summary>
    Task<AddMemberResult> AddMemberByEmailAsync(int boardId, string email, string role);

    /// <summary>Indique si l'utilisateur peut écrire (Owner ou non-Viewer).</summary>
    Task<bool> UserCanWriteAsync(int boardId, int userId);

    /// <summary>
    /// Change le rôle d'un membre. Échoue si l'utilisateur cible est le owner.
    /// </summary>
    Task<ChangeRoleResult> ChangeMemberRoleAsync(int boardId, int targetUserId, string newRole);

    /// <summary>
    /// Retire un membre du board. Échoue si l'utilisateur cible est le owner.
    /// </summary>
    Task<RemoveMemberResult> RemoveMemberAsync(int boardId, int targetUserId);

    /// <summary>
    /// Permet à un membre de quitter le board. Échoue si l'utilisateur est le owner.
    /// </summary>
    Task<LeaveBoardResult> LeaveBoardAsync(int boardId, int userId);

    /// <summary>Retourne le titre d'un board (ou null si introuvable).</summary>
    Task<string?> GetBoardTitleAsync(int boardId);

}