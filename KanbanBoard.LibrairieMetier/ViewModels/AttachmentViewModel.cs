namespace KanbanBoard.LibrairieMetier.ViewModels;

public class AttachmentViewModel
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long? FileSizeKB { get; set; }
    public DateTime UploadedAt { get; set; }

    public int UploadedById { get; set; }
    public string UploadedByUsername { get; set; } = string.Empty;
}