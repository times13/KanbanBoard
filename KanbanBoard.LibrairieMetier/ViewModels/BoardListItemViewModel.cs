namespace KanbanBoard.LibrairieMetier.ViewModels;

public class BoardListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CardCount { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}