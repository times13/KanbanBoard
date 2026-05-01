using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class CardReadDA : ICardReadDA
{
    private readonly AppDbContext _db;

    public CardReadDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task MarkAsReadAsync(int userId, int cardId)
    {
        var existing = await _db.CARD_READs
            .FirstOrDefaultAsync(cr => cr.UserId == userId && cr.CardId == cardId);

        if (existing != null)
        {
            existing.LastReadAt = DateTime.UtcNow;
        }
        else
        {
            _db.CARD_READs.Add(new CARD_READ
            {
                UserId = userId,
                CardId = cardId,
                LastReadAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsForBoardAsync(int boardId, int userId)
    {
        // Récupère, pour chaque carte du board, le LastReadAt du user (ou DateTime.MinValue si jamais lu)
        // Puis compte les commentaires créés après cette date, et qui ne sont pas écrits par le user lui-même

        var query = from card in _db.CARDs
                    where card.Column.BoardId == boardId && !card.IsArchived
                    let lastRead = _db.CARD_READs
                        .Where(cr => cr.UserId == userId && cr.CardId == card.Id)
                        .Select(cr => (DateTime?)cr.LastReadAt)
                        .FirstOrDefault()
                    let unreadCount = _db.COMMENTs
                        .Count(co => co.CardId == card.Id
                                  && co.AuthorId != userId
                                  && (lastRead == null || co.CreatedAt > lastRead))
                    select new { CardId = card.Id, UnreadCount = unreadCount };

        return await query.ToDictionaryAsync(x => x.CardId, x => x.UnreadCount);
    }
}