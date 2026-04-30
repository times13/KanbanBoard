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

    // -- Assignee --
    [Display(Name = "Assigné à")]
    public int? AssigneeId { get; set; }

    /// <summary>Liste des membres du board (pour le dropdown). Rempli côté controller.</summary>
    public List<BoardMemberItemViewModel> AvailableMembers { get; set; } = new();

    /// <summary>Username de l'assignee actuel (pour affichage). Rempli côté controller.</summary>
    public string? CurrentAssigneeUsername { get; set; }

    // -- Commentaires --
    public List<CommentViewModel> Comments { get; set; } = new();

    [StringLength(2000, ErrorMessage = "Le commentaire est trop long (2000 caractères max).")]
    public string? NewComment { get; set; }
}