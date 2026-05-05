using KanbanBoard.LibrairieMetier.ViewModels;

namespace KanbanBoard.LibrairieMetier.Interfaces;

public interface IAttachmentDA
{
    /// <summary>Liste les pièces jointes d'une carte (récentes d'abord).</summary>
    Task<List<AttachmentViewModel>> GetForCardAsync(int cardId);

    /// <summary>Ajoute une pièce jointe et retourne son Id.</summary>
    Task<int> AddAttachmentAsync(int cardId, int uploadedById, string fileName, string fileUrl, long? fileSizeKB);

    /// <summary>Récupère une pièce jointe pour vérifier l'auteur ou le path.</summary>
    Task<AttachmentViewModel?> GetAttachmentAsync(int attachmentId);

    /// <summary>Récupère le BoardId d'une pièce jointe (via Card → Column → Board).</summary>
    Task<int?> GetAttachmentBoardIdAsync(int attachmentId);

    /// <summary>Récupère le path physique du fichier (FileUrl) — utile avant suppression.</summary>
    Task<string?> GetAttachmentFileUrlAsync(int attachmentId);

    /// <summary>Supprime une pièce jointe en base. (La suppression du fichier physique est faite par le controller).</summary>
    Task<bool> DeleteAttachmentAsync(int attachmentId);
}