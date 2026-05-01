using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class BoardDA : IBoardDA
{
    private readonly AppDbContext _db;
    private readonly ICardReadDA _cardReadDA;

    public BoardDA(AppDbContext db, ICardReadDA cardReadDA)
    {
        _db = db;
        _cardReadDA = cardReadDA;
    }

    public async Task<List<BoardListItemViewModel>> GetBoardsForUserAsync(int userId)
    {
        // Boards dont l'utilisateur est owner OU membre
        var boards = await _db.BOARDs
            .Where(b => b.OwnerId == userId
                     || _db.BOARD_MEMBERs.Any(m => m.BoardId == b.Id && m.UserId == userId))
            .Select(b => new BoardListItemViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                CreatedAt = b.CreatedAt,
                CardCount = _db.CARDs.Count(c => c.Column.BoardId == b.Id && !c.IsArchived),
                OwnerUsername = _db.USERs.Where(u => u.Id == b.OwnerId).Select(u => u.Username).FirstOrDefault() ?? "?",
                IsOwner = b.OwnerId == userId
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return boards;
    }

    public async Task<KanbanBoardViewModel?> GetBoardDetailsAsync(int boardId, int userId)
    {
        var hasAccess = await UserHasAccessAsync(boardId, userId);
        if (!hasAccess) return null;

        var board = await _db.BOARDs
            .Where(b => b.Id == boardId)
            .Select(b => new KanbanBoardViewModel
            {
                BoardId = b.Id,
                Title = b.Title,
                Description = b.Description,
                CanEdit = b.OwnerId == userId
                       || _db.BOARD_MEMBERs.Any(m => m.BoardId == b.Id && m.UserId == userId && m.Role != "Viewer"),
                IsAdmin = b.OwnerId == userId
                       || _db.BOARD_MEMBERs.Any(m => m.BoardId == b.Id && m.UserId == userId && m.Role == "Admin")
            })
            .FirstOrDefaultAsync();

        if (board == null) return null;

        // Charger colonnes + cartes
        board.Columns = await _db.BOARD_COLUMNs
    .Where(c => c.BoardId == boardId)
    .OrderBy(c => c.Position)
    .Select(c => new KanbanColumnViewModel
    {
        Id = c.Id,
        Title = c.Title,
        Position = c.Position,
        RowVersion = c.RowVersion,
        Cards = c.CARDs
            .Where(card => !card.IsArchived)
            .OrderBy(card => card.Position)
            .Select(card => new KanbanCardViewModel
            {
                Id = card.Id,
                Title = card.Title,
                Description = card.Description,
                Priority = card.Priority,
                Position = card.Position,
                DueDate = card.DueDate,
                AssigneeId = card.AssigneeId,
                AssigneeUsername = card.Assignee != null ? card.Assignee.Username : null,
                IsArchived = card.IsArchived,
                CommentCount = card.COMMENTs.Count(co => true)
            })
            .ToList()
    })
    .ToListAsync();

        // Récupère les non-lus pour chaque carte du board
        var unreadCounts = await _cardReadDA.GetUnreadCountsForBoardAsync(boardId, userId);

        // Applique les unread counts à chaque carte
        foreach (var col in board.Columns)
        {
            foreach (var card in col.Cards)
            {
                card.UnreadCount = unreadCounts.GetValueOrDefault(card.Id, 0);
            }
        }

        return board;
    }

    public async Task<int> CreateBoardAsync(int ownerId, string title, string? description)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var board = new BOARD
        {
            OwnerId = ownerId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.BOARDs.Add(board);
        await _db.SaveChangesAsync();

        // Créer 3 colonnes par défaut
        var defaults = new[] { "À faire", "En cours", "Terminé" };
        for (int i = 0; i < defaults.Length; i++)
        {
            _db.BOARD_COLUMNs.Add(new BOARD_COLUMN
            {
                BoardId = board.Id,
                Title = defaults[i],
                Position = i
            });
        }

        // L'owner est automatiquement Admin du board
        _db.BOARD_MEMBERs.Add(new BOARD_MEMBER
        {
            BoardId = board.Id,
            UserId = ownerId,
            Role = "Admin",
            JoinedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return board.Id;
    }

    public async Task<bool> UserHasAccessAsync(int boardId, int userId)
    {
        return await _db.BOARDs
            .AnyAsync(b => b.Id == boardId
                        && (b.OwnerId == userId
                         || _db.BOARD_MEMBERs.Any(m => m.BoardId == boardId && m.UserId == userId)));
    }

    public async Task<bool> UserIsAdminAsync(int boardId, int userId)
    {
        return await _db.BOARDs
            .AnyAsync(b => b.Id == boardId
                        && (b.OwnerId == userId
                         || _db.BOARD_MEMBERs.Any(m =>
                                m.BoardId == boardId
                             && m.UserId == userId
                             && m.Role == "Admin")));
    }

    public async Task<List<BoardMemberItemViewModel>> GetMembersAsync(int boardId)
    {
        // Récupère l'owner + les membres explicites, déduplique
        var board = await _db.BOARDs
            .Where(b => b.Id == boardId)
            .Select(b => new { b.OwnerId })
            .FirstOrDefaultAsync();

        if (board == null) return new List<BoardMemberItemViewModel>();

        var members = await _db.BOARD_MEMBERs
            .Where(m => m.BoardId == boardId)
            .Select(m => new BoardMemberItemViewModel
            {
                UserId = m.UserId,
                Username = m.User.Username,
                Email = m.User.Email,
                Role = m.Role
            })
            .ToListAsync();

        // S'assurer que l'owner est bien dans la liste (au cas où il ne serait pas dans BOARD_MEMBER)
        if (!members.Any(m => m.UserId == board.OwnerId))
        {
            var owner = await _db.USERs
                .Where(u => u.Id == board.OwnerId)
                .Select(u => new BoardMemberItemViewModel
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = "Admin"
                })
                .FirstOrDefaultAsync();

            if (owner != null) members.Insert(0, owner);
        }

        return members.OrderBy(m => m.Username).ToList();
    }
}