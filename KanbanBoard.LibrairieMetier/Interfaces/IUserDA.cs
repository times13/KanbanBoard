using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IUserDA
{
    /// <summary>Récupère l'Id d'un utilisateur par son email (case-insensitive). null si inexistant.</summary>
    Task<int?> GetUserIdByEmailAsync(string email);

    /// <summary>Récupère le username d'un utilisateur par son Id. null si inexistant.</summary>
    Task<string?> GetUsernameAsync(int userId);

    /// <summary>
    /// Recherche des utilisateurs par username ou email (LIKE %query%),
    /// en excluant ceux qui sont déjà membres ou owner du board donné.
    /// Retourne max 10 résultats.
    /// </summary>
    Task<List<UserSearchResultViewModel>> SearchAvailableUsersAsync(int boardId, string query);
}