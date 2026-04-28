using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class COMMENT
{
    public int Id { get; set; }

    public int CardId { get; set; }

    public int AuthorId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual USER Author { get; set; } = null!;

    public virtual CARD Card { get; set; } = null!;
}
