using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class USER
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsGlobalAdmin { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ACTIVITY_LOG> ACTIVITY_LOGs { get; set; } = new List<ACTIVITY_LOG>();

    public virtual ICollection<ATTACHMENT> ATTACHMENTs { get; set; } = new List<ATTACHMENT>();

    public virtual ICollection<BOARD_MEMBER> BOARD_MEMBERs { get; set; } = new List<BOARD_MEMBER>();

    public virtual ICollection<BOARD> BOARDs { get; set; } = new List<BOARD>();

    public virtual ICollection<CARD> CARDArchivedBies { get; set; } = new List<CARD>();

    public virtual ICollection<CARD> CARDAssignees { get; set; } = new List<CARD>();

    public virtual ICollection<COMMENT> COMMENTs { get; set; } = new List<COMMENT>();

    public virtual ICollection<NOTIFICATION> NOTIFICATIONActors { get; set; } = new List<NOTIFICATION>();

    public virtual ICollection<NOTIFICATION> NOTIFICATIONUsers { get; set; } = new List<NOTIFICATION>();
}
