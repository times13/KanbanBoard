using System.ComponentModel.DataAnnotations;

namespace KanbanBoard.LibrairieMetier.ViewModels;

public class CreateBoardViewModel
{
    [Required(ErrorMessage = "Le titre est requis.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Le titre doit faire entre 3 et 200 caractères.")]
    [Display(Name = "Titre du tableau")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Description (optionnelle)")]
    public string? Description { get; set; }
}
