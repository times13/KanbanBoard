using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class LABEL
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public string Name { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual BOARD Board { get; set; } = null!;

    public virtual ICollection<CARD> Cards { get; set; } = new List<CARD>();
}
