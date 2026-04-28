using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class BOARD_COLUMN
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public string Title { get; set; } = null!;

    public int Position { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual BOARD Board { get; set; } = null!;

    public virtual ICollection<CARD> CARDs { get; set; } = new List<CARD>();
}
