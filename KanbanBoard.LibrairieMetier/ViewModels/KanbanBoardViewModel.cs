namespace KanbanBoard.LibrairieMetier.ViewModels;

public class KanbanBoardViewModel
{
    public int BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool CanEdit { get; set; }   // l'utilisateur courant peut-il modifier ?

    /// <summary>Peut gérer les colonnes (owner ou Admin).</summary>
    public bool IsAdmin { get; set; }

    public List<KanbanColumnViewModel> Columns { get; set; } = new();
}

public class KanbanColumnViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public List<KanbanCardViewModel> Cards { get; set; } = new();
}

public class KanbanCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public int Position { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssigneeUsername { get; set; }
    public bool IsArchived { get; set; }
}
