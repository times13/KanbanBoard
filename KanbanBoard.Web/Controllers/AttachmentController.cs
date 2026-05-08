using KanbanBoard.LibrairieMetier.Constants;
using KanbanBoard.LibrairieMetier.Interfaces;
using KanbanBoard.Web.Hubs;
using KanbanBoard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace KanbanBoard.Web.Controllers;

[Authorize]
public class AttachmentController : Controller
{
    private readonly IAttachmentDA _attachmentDA;
    private readonly ICardDA _cardDA;
    private readonly IBoardDA _boardDA;
    private readonly IWebHostEnvironment _env;
    private readonly NotificationService _notif;
    private readonly ActivityLogService _activityLog;
    private readonly IHubContext<KanbanHub> _hub;

    // Constantes de validation
    private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] ALLOWED_EXTENSIONS = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf",
        ".doc", ".docx",
        ".xls", ".xlsx",
        ".ppt", ".pptx",
        ".txt"
    };

    public AttachmentController(
        IAttachmentDA attachmentDA,
        ICardDA cardDA,
        IBoardDA boardDA,
        IWebHostEnvironment env,
        NotificationService notif,
        ActivityLogService activityLog,
        IHubContext<KanbanHub> hub)
    {
        _attachmentDA = attachmentDA;
        _cardDA = cardDA;
        _boardDA = boardDA;
        _env = env;
        _notif = notif;
        _activityLog = activityLog;
        _hub = hub;
    }

    // ---------- UPLOAD ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)] // 11 MB côté Kestrel pour avoir une marge
    public async Task<IActionResult> Upload(int cardId, int boardId, IFormFile file)
    {
        var userId = GetCurrentUserId();

        // Vérification : l'utilisateur peut écrire (Admin/Member, pas Viewer)
        if (!await _boardDA.UserCanWriteAsync(boardId, userId))
        {
            TempData["ErrorMessage"] = "Vous êtes en lecture seule, vous ne pouvez pas ajouter de pièce jointe.";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        // Validation : fichier présent
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Aucun fichier sélectionné.";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        // Validation : taille
        if (file.Length > MAX_FILE_SIZE)
        {
            TempData["ErrorMessage"] = "Le fichier dépasse la taille maximum (10 MB).";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        // Validation : extension
        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !ALLOWED_EXTENSIONS.Contains(extension))
        {
            TempData["ErrorMessage"] = $"Type de fichier non autorisé ({extension}). Autorisés : images, PDF, Office, txt.";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        // Génération d'un nom unique : <GUID>_<nom_original>.<ext>
        var safeFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalFileName));
        var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}{extension}";

        // Path physique sur disque
        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsRoot); // créé si pas déjà
        var physicalPath = Path.Combine(uploadsRoot, uniqueFileName);

        // Path web (pour FileUrl)
        var webPath = $"/uploads/{uniqueFileName}";

        // Sauvegarde sur disque
        try
        {
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Erreur lors de la sauvegarde : {ex.Message}";
            return RedirectToAction("Edit", "Card", new { id = cardId });
        }

        // INSERT en base
        var sizeKB = (long)Math.Ceiling(file.Length / 1024.0);
        await _attachmentDA.AddAttachmentAsync(
            cardId: cardId,
            uploadedById: userId,
            fileName: originalFileName,
            fileUrl: webPath,
            fileSizeKB: sizeKB);

        var card = await _cardDA.GetCardAsync(cardId);
        var cardTitle = card?.Title ?? "?";

        TempData["SuccessMessage"] = $"Fichier « {originalFileName} » ajouté.";
        await _activityLog.LogAsync(
            boardId: boardId,
            userId: userId,
            entityType: ActivityEntityType.Attachment,
            entityId: cardId, // on log l'Id de la carte
            action: ActivityAction.AttachmentUploaded,
            details: $"\"{originalFileName}\" sur \"{cardTitle}\"");

        // Notifier l'assignee de la carte (si différent de l'uploader)
        if (card?.AssigneeId.HasValue == true && card.AssigneeId.Value != userId)
        {
            var boardTitle = await _boardDA.GetBoardTitleAsync(boardId) ?? "(sans titre)";
            await _notif.NotifyUserAsync(
                userId: card.AssigneeId.Value,
                actorId: userId,
                type: "AttachmentAdded",
                message: $"{User.Identity?.Name} a ajouté « {originalFileName} » à la carte « {card.Title} » ({boardTitle})",
                boardId: boardId,
                cardId: cardId);
        }

        // Broadcast SignalR aux autres membres connectés
        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId))
            .SendAsync("BoardChanged", new
            {
                action = "AttachmentAdded",
                cardId = cardId,
                fileName = originalFileName,
                triggeredBy = User.Identity?.Name
            });

        return RedirectToAction("Edit", "Card", new { id = cardId });
    }

    // ---------- DOWNLOAD ----------

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var userId = GetCurrentUserId();
        var attachment = await _attachmentDA.GetAttachmentAsync(id);

        if (attachment == null)
            return NotFound();

        var boardId = await _attachmentDA.GetAttachmentBoardIdAsync(id);
        if (boardId == null) return NotFound();

        // Vérification : l'utilisateur a accès au board (même Viewer peut télécharger)
        if (!await _boardDA.UserHasAccessAsync(boardId.Value, userId))
            return Forbid();

        // Path physique sur disque
        var fileName = Path.GetFileName(attachment.FileUrl); // récupère <guid>_<nom>.<ext>
        var physicalPath = Path.Combine(_env.WebRootPath, "uploads", fileName);

        if (!System.IO.File.Exists(physicalPath))
        {
            TempData["ErrorMessage"] = "Fichier introuvable sur le serveur.";
            return RedirectToAction("Edit", "Card", new { id = attachment.CardId });
        }

        // Lecture et renvoi du fichier
        var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        var contentType = GetContentType(attachment.FileName);
        return File(bytes, contentType, attachment.FileName);
    }

    // ---------- DELETE ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var attachment = await _attachmentDA.GetAttachmentAsync(id);

        if (attachment == null) return NotFound();

        var boardId = await _attachmentDA.GetAttachmentBoardIdAsync(id);
        if (boardId == null) return NotFound();

        // Auteur OU Admin peut supprimer
        var isOwner = attachment.UploadedById == userId;
        var isAdmin = await _boardDA.UserIsAdminAsync(boardId.Value, userId);

        if (!isOwner && !isAdmin)
        {
            TempData["ErrorMessage"] = "Vous ne pouvez pas supprimer ce fichier (réservé à l'auteur ou un admin).";
            return RedirectToAction("Edit", "Card", new { id = attachment.CardId });
        }

        // Supprimer le fichier physique
        try
        {
            var physicalPath = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(attachment.FileUrl));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }
        catch
        {
            // On continue même si la suppression du fichier physique échoue
            // (peut-être déjà supprimé manuellement)
        }

        var card = attachment != null ? await _cardDA.GetCardAsync(attachment.CardId) : null;
        var cardTitle = card?.Title ?? "?";
        var fileName = attachment?.FileName ?? "?";
        // Supprimer la ligne en base
        await _attachmentDA.DeleteAttachmentAsync(id);

        TempData["SuccessMessage"] = "Pièce jointe supprimée.";

        await _activityLog.LogAsync(
            boardId: boardId.Value,
            userId: userId,
            entityType: ActivityEntityType.Attachment,
            entityId: attachment?.CardId,
            action: ActivityAction.AttachmentDeleted,
            details: $"\"{fileName}\" de \"{cardTitle}\"");

        await _hub.Clients
            .Group(KanbanHub.BoardGroupName(boardId.Value))
            .SendAsync("BoardChanged", new
            {
                action = "AttachmentDeleted",
                cardId = attachment?.CardId,
                triggeredBy = User.Identity?.Name
            });

        return RedirectToAction("Edit", "Card", new { id = attachment?.CardId });
    }

    // ---------- HELPERS ----------

    /// <summary>
    /// Nettoie un nom de fichier pour éviter les caractères dangereux.
    /// </summary>
    private static string SanitizeFileName(string name)
    {
        // Garde uniquement lettres, chiffres, tirets et underscores
        var safe = string.Concat(name.Where(c =>
            char.IsLetterOrDigit(c) || c == '-' || c == '_'));

        // Si le nom est vide après nettoyage, on met "fichier"
        return string.IsNullOrEmpty(safe) ? "fichier" : safe;
    }

    /// <summary>
    /// Devine le ContentType à partir de l'extension.
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException("Utilisateur non identifié.");
        return id;
    }
}