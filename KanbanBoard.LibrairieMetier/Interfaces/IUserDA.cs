namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IUserDA
{
    /// <summary>Récupère l'Id d'un utilisateur par son email (case-insensitive). null si inexistant.</summary>
    Task<int?> GetUserIdByEmailAsync(string email);

    /// <summary>Récupère le username d'un utilisateur par son Id. null si inexistant.</summary>
    Task<string?> GetUsernameAsync(int userId);
}