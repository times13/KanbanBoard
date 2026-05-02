using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.LibrairieMetier.Interfaces;
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
}