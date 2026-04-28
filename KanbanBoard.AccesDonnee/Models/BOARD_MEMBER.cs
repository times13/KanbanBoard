using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class BOARD_MEMBER
{
    public int BoardId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public DateTime JoinedAt { get; set; }

    public virtual BOARD Board { get; set; } = null!;

    public virtual USER User { get; set; } = null!;
}
