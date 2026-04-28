using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class NOTIFICATION
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ActorId { get; set; }

    public int? CardId { get; set; }

    public int? BoardId { get; set; }

    public string Type { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual USER Actor { get; set; } = null!;

    public virtual BOARD? Board { get; set; }

    public virtual CARD? Card { get; set; }

    public virtual USER User { get; set; } = null!;
}
