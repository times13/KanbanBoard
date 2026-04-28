using System;
using System.Collections.Generic;

namespace KanbanBoard.AccesDonnee.Models;

public partial class ATTACHMENT
{
    public int Id { get; set; }

    public int CardId { get; set; }

    public int UploadedById { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public long? FileSizeKB { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual CARD Card { get; set; } = null!;

    public virtual USER UploadedBy { get; set; } = null!;
}
