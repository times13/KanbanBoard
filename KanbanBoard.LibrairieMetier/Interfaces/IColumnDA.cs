namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IColumnDA
{
    Task<int> CreateColumnAsync(int boardId, string title);
    Task<bool> RenameColumnAsync(int columnId, string newTitle);
    Task<bool> DeleteColumnAsync(int columnId);
}