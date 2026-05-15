using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class LabelDA : ILabelDA
{
    private readonly AppDbContext _db;

    public LabelDA(AppDbContext db)
    {
        _db = db;
    }

    // ---------- Gestion des labels d'un board ----------

    public async Task<List<LabelViewModel>> GetForBoardAsync(int boardId)
    {
        return await _db.LABELs
            .Where(l => l.BoardId == boardId)
            .Select(l => new LabelViewModel
            {
                Id = l.Id,
                BoardId = l.BoardId,
                Name = l.Name,
                Color = l.Color,
                CardCount = l.Cards.Count
            })
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<LabelViewModel?> GetByIdAsync(int labelId)
    {
        return await _db.LABELs
            .Where(l => l.Id == labelId)
            .Select(l => new LabelViewModel
            {
                Id = l.Id,
                BoardId = l.BoardId,
                Name = l.Name,
                Color = l.Color,
                CardCount = l.Cards.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(int boardId, string name, string color)
    {
        var label = new LABEL
        {
            BoardId = boardId,
            Name = name.Trim(),
            Color = color
        };

        _db.LABELs.Add(label);
        await _db.SaveChangesAsync();
        return label.Id;
    }

    public async Task<bool> UpdateAsync(int labelId, string name, string color)
    {
        var label = await _db.LABELs.FindAsync(labelId);
        if (label == null) return false;

        label.Name = name.Trim();
        label.Color = color;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int labelId)
    {
        var label = await _db.LABELs.FindAsync(labelId);
        if (label == null) return false;

        // Le CASCADE sur CardId dans CARD_LABEL nettoie automatiquement les liens
        // côté carte, mais on a NO_ACTION sur LabelId. On nettoie manuellement.
        var cardLabels = await _db.Set<Dictionary<string, object>>("CARD_LABEL")
            .Where(cl => (int)cl["LabelId"] == labelId)
            .ToListAsync();

        foreach (var cl in cardLabels)
            _db.Remove(cl);

        _db.LABELs.Remove(label);
        await _db.SaveChangesAsync();
        return true;
    }

    // ---------- Assignment aux cartes ----------

    public async Task<List<LabelViewModel>> GetForCardAsync(int cardId)
    {
        return await _db.LABELs
            .Where(l => l.Cards.Any(c => c.Id == cardId))
            .Select(l => new LabelViewModel
            {
                Id = l.Id,
                BoardId = l.BoardId,
                Name = l.Name,
                Color = l.Color
            })
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<bool> AssignToCardAsync(int cardId, int labelId)
    {
        // Vérifier que la carte et le label existent
        var card = await _db.CARDs
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(c => c.Id == cardId);
        if (card == null) return false;

        var label = await _db.LABELs.FindAsync(labelId);
        if (label == null) return false;

        // Idempotent : si déjà assigné, ne rien faire
        if (card.Labels.Any(l => l.Id == labelId))
            return true;

        card.Labels.Add(label);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnassignFromCardAsync(int cardId, int labelId)
    {
        var card = await _db.CARDs
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(c => c.Id == cardId);
        if (card == null) return false;

        var label = card.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label == null) return true; // idempotent

        card.Labels.Remove(label);
        await _db.SaveChangesAsync();
        return true;
    }

    // ---------- Helpers ----------

    public async Task<List<(LabelViewModel Label, bool IsAssigned)>> GetForBoardWithAssignmentAsync(int boardId, int cardId)
    {
        var labels = await GetForBoardAsync(boardId);
        var assignedIds = await _db.LABELs
            .Where(l => l.Cards.Any(c => c.Id == cardId))
            .Select(l => l.Id)
            .ToListAsync();

        return labels
            .Select(l => (l, assignedIds.Contains(l.Id)))
            .ToList();
    }

    public async Task<int?> GetBoardIdAsync(int labelId)
    {
        var label = await _db.LABELs
            .Where(l => l.Id == labelId)
            .Select(l => (int?)l.BoardId)
            .FirstOrDefaultAsync();
        return label;
    }
}