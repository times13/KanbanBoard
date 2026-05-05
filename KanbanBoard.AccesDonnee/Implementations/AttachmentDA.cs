using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.LibrairieMetier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.Implementations;

public class AttachmentDA : IAttachmentDA
{
    private readonly AppDbContext _db;

    public AttachmentDA(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AttachmentViewModel>> GetForCardAsync(int cardId)
    {
        return await _db.ATTACHMENTs
            .Where(a => a.CardId == cardId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                CardId = a.CardId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileSizeKB = a.FileSizeKB,
                UploadedAt = a.UploadedAt,
                UploadedById = a.UploadedById,
                UploadedByUsername = a.UploadedBy.Username
            })
            .ToListAsync();
    }

    public async Task<int> AddAttachmentAsync(int cardId, int uploadedById, string fileName, string fileUrl, long? fileSizeKB)
    {
        var attachment = new ATTACHMENT
        {
            CardId = cardId,
            UploadedById = uploadedById,
            FileName = fileName,
            FileUrl = fileUrl,
            FileSizeKB = fileSizeKB,
            UploadedAt = DateTime.UtcNow
        };

        _db.ATTACHMENTs.Add(attachment);
        await _db.SaveChangesAsync();
        return attachment.Id;
    }

    public async Task<AttachmentViewModel?> GetAttachmentAsync(int attachmentId)
    {
        return await _db.ATTACHMENTs
            .Where(a => a.Id == attachmentId)
            .Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                CardId = a.CardId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileSizeKB = a.FileSizeKB,
                UploadedAt = a.UploadedAt,
                UploadedById = a.UploadedById,
                UploadedByUsername = a.UploadedBy.Username
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetAttachmentBoardIdAsync(int attachmentId)
    {
        return await _db.ATTACHMENTs
            .Where(a => a.Id == attachmentId)
            .Select(a => (int?)a.Card.Column.BoardId)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetAttachmentFileUrlAsync(int attachmentId)
    {
        return await _db.ATTACHMENTs
            .Where(a => a.Id == attachmentId)
            .Select(a => a.FileUrl)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await _db.ATTACHMENTs.FindAsync(attachmentId);
        if (attachment == null) return false;

        _db.ATTACHMENTs.Remove(attachment);
        await _db.SaveChangesAsync();
        return true;
    }
}