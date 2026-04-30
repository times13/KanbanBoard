using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class CommentDA : ICommentDA
{
    private readonly AppDbContext _db;

    public CommentDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CommentViewModel>> GetForCardAsync(int cardId)
    {
        return await _db.COMMENTs
            .Where(c => c.CardId == cardId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentViewModel
            {
                Id = c.Id,
                CardId = c.CardId,
                AuthorId = c.AuthorId,
                AuthorUsername = c.Author.Username,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<int> AddCommentAsync(int cardId, int authorId, string content)
    {
        var comment = new COMMENT
        {
            CardId = cardId,
            AuthorId = authorId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.COMMENTs.Add(comment);
        await _db.SaveChangesAsync();
        return comment.Id;
    }

    public async Task<CommentViewModel?> GetCommentAsync(int commentId)
    {
        return await _db.COMMENTs
            .Where(c => c.Id == commentId)
            .Select(c => new CommentViewModel
            {
                Id = c.Id,
                CardId = c.CardId,
                AuthorId = c.AuthorId,
                AuthorUsername = c.Author.Username,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetCommentBoardIdAsync(int commentId)
    {
        return await _db.COMMENTs
            .Where(c => c.Id == commentId)
            .Select(c => (int?)c.Card.Column.BoardId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _db.COMMENTs.FindAsync(commentId);
        if (comment == null) return false;

        _db.COMMENTs.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }
}