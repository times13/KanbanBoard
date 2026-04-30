namespace KanbanBoard.LibrairieMetier.ViewModels;

public class CommentViewModel
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}