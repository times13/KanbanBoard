namespace KanbanBoard.LibrairieMetier.ViewModels;

public class BoardMemberItemViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}