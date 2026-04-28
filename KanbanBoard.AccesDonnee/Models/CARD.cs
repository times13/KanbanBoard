using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class CARD
{
    public int Id { get; set; }

    public int ColumnId { get; set; }

    public int? AssigneeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Priority { get; set; } = null!;

    public int Position { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public bool IsArchived { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public int? ArchivedById { get; set; }

    public virtual ICollection<ATTACHMENT> ATTACHMENTs { get; set; } = new List<ATTACHMENT>();

    public virtual USER? ArchivedBy { get; set; }

    public virtual USER? Assignee { get; set; }

    public virtual ICollection<COMMENT> COMMENTs { get; set; } = new List<COMMENT>();

    public virtual BOARD_COLUMN Column { get; set; } = null!;

    public virtual ICollection<NOTIFICATION> NOTIFICATIONs { get; set; } = new List<NOTIFICATION>();

    public virtual ICollection<LABEL> Labels { get; set; } = new List<LABEL>();
}
