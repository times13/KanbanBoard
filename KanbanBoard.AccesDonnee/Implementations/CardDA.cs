using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class CardDA : ICardDA
{
    private readonly AppDbContext _db;

    public CardDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateCardAsync(int columnId, string title, string? description, int createdByUserId)
    {
        var maxPos = await _db.CARDs
            .Where(c => c.ColumnId == columnId)
            .MaxAsync(c => (int?)c.Position) ?? -1;

        var card = new CARD
        {
            ColumnId = columnId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Priority = "Medium",
            Position = maxPos + 1,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };

        _db.CARDs.Add(card);
        await _db.SaveChangesAsync();
        return card.Id;
    }

    public async Task<KanbanCardViewModel?> GetCardAsync(int cardId)
    {
        return await _db.CARDs
            .Where(c => c.Id == cardId)
            .Select(c => new KanbanCardViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Priority = c.Priority,
                Position = c.Position,
                DueDate = c.DueDate,
                AssigneeUsername = c.Assignee != null ? c.Assignee.Username : null,
                IsArchived = c.IsArchived
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateCardAsync(int cardId, string title, string? description, string priority, DateTime? dueDate, int? assigneeId)
    {
        var card = await _db.CARDs.FindAsync(cardId);
        if (card == null) return false;

        card.Title = title.Trim();
        card.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        card.Priority = priority;
        card.DueDate = dueDate;
        card.AssigneeId = assigneeId;
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCardAsync(int cardId)
    {
        var card = await _db.CARDs.FindAsync(cardId);
        if (card == null) return false;

        _db.CARDs.Remove(card);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveCardAsync(int cardId, int targetColumnId, int newPosition)
    {
        var card = await _db.CARDs.FindAsync(cardId);
        if (card == null) return false;

        card.ColumnId = targetColumnId;
        card.Position = newPosition;
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }
}