namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IColumnDA
{
    Task<int> CreateColumnAsync(int boardId, string title);
    Task<bool> RenameColumnAsync(int columnId, string newTitle);

    /// <summary>Supprime la colonne ET toutes ses cartes (cascade SQL).</summary>
    Task<bool> DeleteColumnAsync(int columnId);

    /// <summary>Récupère le BoardId d'une colonne (utile pour autorisation).</summary>
    Task<int?> GetColumnBoardIdAsync(int columnId);

    /// <summary>Compte les cartes non-archivées d'une colonne (pour confirmation suppression).</summary>
    Task<int> CountCardsAsync(int columnId);

    /// <summary>Retourne le titre d'une colonne ou null si introuvable.</summary>
    Task<string?> GetColumnTitleAsync(int columnId);
}