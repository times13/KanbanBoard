using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class ActivityLogDA : IActivityLogDA
{
    private const int MAX_ENTRIES_PER_BOARD = 100;
    private readonly AppDbContext _db;

    public ActivityLogDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int boardId, int userId, string entityType, int? entityId, string action, string? details = null)
    {
        // 1. Création du log
        var log = new ACTIVITY_LOG
        {
            BoardId = boardId,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Details = details,
            OccurredAt = DateTime.UtcNow
        };
        _db.ACTIVITY_LOGs.Add(log);
        await _db.SaveChangesAsync();

        // 2. Purge déclenchée 1 fois sur 10 pour économiser les requêtes SQL
        if (Random.Shared.Next(10) == 0)
        {
            var currentLogs = await _db.ACTIVITY_LOGs
                .Where(l => l.BoardId == boardId)
                .OrderByDescending(l => l.OccurredAt)
                .Skip(MAX_ENTRIES_PER_BOARD)
                .ToListAsync();

            if (currentLogs.Any())
            {
                _db.ACTIVITY_LOGs.RemoveRange(currentLogs);
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<List<ActivityLogItemViewModel>> GetForBoardAsync(int boardId, int limit = 20)
    {
        return await _db.ACTIVITY_LOGs
            .Where(l => l.BoardId == boardId)
            .OrderByDescending(l => l.OccurredAt)
            .Take(limit)
            .Select(l => new ActivityLogItemViewModel
            {
                Id = l.Id,
                BoardId = l.BoardId,
                UserId = l.UserId,
                Username = l.User.Username,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Action = l.Action,
                OccurredAt = l.OccurredAt,
                EntityName = l.Details
            })
            .ToListAsync();
    }
}