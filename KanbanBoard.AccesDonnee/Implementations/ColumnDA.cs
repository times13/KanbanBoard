using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class ColumnDA : IColumnDA
{
    private readonly AppDbContext _db;

    public ColumnDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateColumnAsync(int boardId, string title)
    {
        var maxPos = await _db.BOARD_COLUMNs
            .Where(c => c.BoardId == boardId)
            .MaxAsync(c => (int?)c.Position) ?? -1;

        var column = new BOARD_COLUMN
        {
            BoardId = boardId,
            Title = title.Trim(),
            Position = maxPos + 1
        };

        _db.BOARD_COLUMNs.Add(column);
        await _db.SaveChangesAsync();
        return column.Id;
    }

    public async Task<bool> RenameColumnAsync(int columnId, string newTitle)
    {
        var col = await _db.BOARD_COLUMNs.FindAsync(columnId);
        if (col == null) return false;

        col.Title = newTitle.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteColumnAsync(int columnId)
    {
        var col = await _db.BOARD_COLUMNs.FindAsync(columnId);
        if (col == null) return false;

        // Cascade SQL Server supprimera automatiquement les cartes
        _db.BOARD_COLUMNs.Remove(col);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int?> GetColumnBoardIdAsync(int columnId)
    {
        return await _db.BOARD_COLUMNs
            .Where(c => c.Id == columnId)
            .Select(c => (int?)c.BoardId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountCardsAsync(int columnId)
    {
        return await _db.CARDs
            .CountAsync(c => c.ColumnId == columnId && !c.IsArchived);
    }
}