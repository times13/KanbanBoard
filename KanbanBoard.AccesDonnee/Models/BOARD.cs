using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class BOARD
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ACTIVITY_LOG> ACTIVITY_LOGs { get; set; } = new List<ACTIVITY_LOG>();

    public virtual ICollection<BOARD_COLUMN> BOARD_COLUMNs { get; set; } = new List<BOARD_COLUMN>();

    public virtual ICollection<BOARD_MEMBER> BOARD_MEMBERs { get; set; } = new List<BOARD_MEMBER>();

    public virtual ICollection<LABEL> LABELs { get; set; } = new List<LABEL>();

    public virtual ICollection<NOTIFICATION> NOTIFICATIONs { get; set; } = new List<NOTIFICATION>();

    public virtual USER Owner { get; set; } = null!;
}
