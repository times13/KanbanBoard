using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class ACTIVITY_LOG
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public int UserId { get; set; }

    public string EntityType { get; set; } = null!;

    public int? EntityId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public virtual BOARD Board { get; set; } = null!;

    public virtual USER User { get; set; } = null!;
}
