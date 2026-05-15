using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface ILabelDA
{
    // ---------- Gestion des labels d'un board ----------
    Task<List<LabelViewModel>> GetForBoardAsync(int boardId);
    Task<LabelViewModel?> GetByIdAsync(int labelId);
    Task<int> CreateAsync(int boardId, string name, string color);
    Task<bool> UpdateAsync(int labelId, string name, string color);
    Task<bool> DeleteAsync(int labelId);

    // ---------- Assignment aux cartes ----------
    Task<List<LabelViewModel>> GetForCardAsync(int cardId);
    Task<bool> AssignToCardAsync(int cardId, int labelId);
    Task<bool> UnassignFromCardAsync(int cardId, int labelId);

    // ---------- Helpers ----------
    /// <summary>Retourne tous les labels du board, avec un flag indiquant s'ils sont assignés à la carte donnée.</summary>
    Task<List<(LabelViewModel Label, bool IsAssigned)>> GetForBoardWithAssignmentAsync(int boardId, int cardId);

    /// <summary>Retourne le BoardId du label (utile pour vérifier l'accès).</summary>
    Task<int?> GetBoardIdAsync(int labelId);
}