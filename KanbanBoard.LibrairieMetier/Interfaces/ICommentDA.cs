using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface ICommentDA
{
    /// <summary>Liste les commentaires d'une carte, du plus récent au plus ancien.</summary>
    Task<List<CommentViewModel>> GetForCardAsync(int cardId);

    /// <summary>Ajoute un commentaire et retourne son Id.</summary>
    Task<int> AddCommentAsync(int cardId, int authorId, string content);

    /// <summary>Récupère un commentaire (utile pour vérifier l'auteur avant suppression).</summary>
    Task<CommentViewModel?> GetCommentAsync(int commentId);

    /// <summary>Récupère le BoardId d'un commentaire (via Card → Column → Board).</summary>
    Task<int?> GetCommentBoardIdAsync(int commentId);

    /// <summary>Supprime un commentaire.</summary>
    Task<bool> DeleteCommentAsync(int commentId);
}