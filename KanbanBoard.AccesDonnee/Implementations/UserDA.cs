using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class UserDA : IUserDA
{
    private readonly AppDbContext _db;

    public UserDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int?> GetUserIdByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _db.USERs
            .Where(u => u.Email == normalizedEmail)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetUsernameAsync(int userId)
    {
        return await _db.USERs
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync();
    }

    public async Task<List<UserSearchResultViewModel>> SearchAvailableUsersAsync(int boardId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            return new List<UserSearchResultViewModel>();

        var normalized = query.Trim().ToLowerInvariant();

        // Récupère les Ids des users déjà membres ou owner du board
        var existingUserIds = new HashSet<int>();

        var ownerId = await _db.BOARDs
            .Where(b => b.Id == boardId)
            .Select(b => (int?)b.OwnerId)
            .FirstOrDefaultAsync();

        if (ownerId.HasValue)
            existingUserIds.Add(ownerId.Value);

        var memberIds = await _db.BOARD_MEMBERs
            .Where(m => m.BoardId == boardId)
            .Select(m => m.UserId)
            .ToListAsync();

        foreach (var id in memberIds)
            existingUserIds.Add(id);

        // Recherche : exclu les Ids déjà membres + LIKE sur username ou email
        return await _db.USERs
            .Where(u => !existingUserIds.Contains(u.Id)
                     && (u.Username.ToLower().Contains(normalized)
                      || u.Email.ToLower().Contains(normalized)))
            .OrderBy(u => u.Username)
            .Take(10)
            .Select(u => new UserSearchResultViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();
    }
}