using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class NotificationDA : INotificationDA
{
    private readonly AppDbContext _db;

    public NotificationDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(
        int userId,
        int actorId,
        string type,
        string message,
        int? boardId = null,
        int? cardId = null)
    {
        var notif = new NOTIFICATION
        {
            UserId = userId,
            ActorId = actorId,
            Type = type,
            Message = message,
            BoardId = boardId,
            CardId = cardId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.NOTIFICATIONs.Add(notif);
        await _db.SaveChangesAsync();
        return notif.Id;
    }

    public async Task<List<NotificationItemViewModel>> GetRecentForUserAsync(int userId, int limit = 10)
    {
        return await _db.NOTIFICATIONs
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationItemViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                BoardId = n.BoardId,
                CardId = n.CardId,
                ActorUsername = n.Actor != null ? n.Actor.Username : null
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.NOTIFICATIONs
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notif = await _db.NOTIFICATIONs
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notif == null) return false;

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var notifs = await _db.NOTIFICATIONs
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifs)
            n.IsRead = true;

        await _db.SaveChangesAsync();
        return notifs.Count;
    }
}