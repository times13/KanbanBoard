using System.ComponentModel.DataAnnotations;

namespace KanbanBoard.LibrairieMetier.ViewModels;

public class EditCardViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int BoardId { get; set; }

    [Required(ErrorMessage = "Le titre est requis.")]
    [StringLength(200, MinimumLength = 2)]
    [Display(Name = "Titre")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Priorité")]
    public string Priority { get; set; } = "Medium";

    [DataType(DataType.Date)]
    [Display(Name = "Date d'échéance")]
    public DateTime? DueDate { get; set; }
}