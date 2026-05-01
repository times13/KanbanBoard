using System.ComponentModel.DataAnnotations;

namespace KanbanBoard.LibrairieMetier.ViewModels;

public class AddMemberViewModel
{
    [Required]
    public int BoardId { get; set; }

    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [Display(Name = "Email du membre")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le rôle est requis.")]
    [Display(Name = "Rôle")]
    public string Role { get; set; } = "Member";
}