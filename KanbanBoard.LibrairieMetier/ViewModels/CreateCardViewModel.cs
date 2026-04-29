using System.ComponentModel.DataAnnotations;

namespace KanbanBoard.LibrairieMetier.ViewModels;

public class CreateCardViewModel
{
    [Required]
    public int ColumnId { get; set; }

    [Required]
    public int BoardId { get; set; }    // Pour la redirection après création

    [Required(ErrorMessage = "Le titre est requis.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Le titre doit faire entre 2 et 200 caractères.")]
    [Display(Name = "Titre")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "Description (facultative)")]
    public string? Description { get; set; }
}