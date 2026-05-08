namespace KanbanBoard.LibrairieMetier.ViewModels;

public class ActivityLogItemViewModel
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }

    /// <summary>Détails optionnels (titre de la carte, nom du fichier, etc.) pour l'affichage.</summary>
    public string? EntityName { get; set; }
}