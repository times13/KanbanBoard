using System.ComponentModel.DataAnnotations;

namespace KanbanBoard.LibrairieMetier.ViewModels;

public class LabelViewModel
{
    public int Id { get; set; }

    [Required]
    public int BoardId { get; set; }

    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Le nom doit faire entre 1 et 50 caractères.")]
    [Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La couleur est requise.")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "La couleur doit être au format hexadécimal (#RRGGBB).")]
    [Display(Name = "Couleur")]
    public string Color { get; set; } = "#0d6efd";   // bleu Bootstrap par défaut

    /// <summary>Nombre de cartes qui utilisent ce label (rempli côté DA).</summary>
    public int CardCount { get; set; }
}