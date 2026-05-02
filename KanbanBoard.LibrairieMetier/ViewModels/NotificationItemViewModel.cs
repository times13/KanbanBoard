namespace KanbanBoard.LibrairieMetier.ViewModels;

public class NotificationItemViewModel
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    // Liens optionnels selon le type
    public int? BoardId { get; set; }
    public int? CardId { get; set; }
    public string? ActorUsername { get; set; }
}