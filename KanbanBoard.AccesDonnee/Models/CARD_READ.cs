using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KanbanBoard.AccesDonnee.Models;

[Table("CARD_READ")]
[PrimaryKey(nameof(UserId), nameof(CardId))]
public partial class CARD_READ
{
    public int UserId { get; set; }
    public int CardId { get; set; }
    public DateTime LastReadAt { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty(nameof(USER.CARD_READs))]
    public virtual USER User { get; set; } = null!;

    [ForeignKey(nameof(CardId))]
    [InverseProperty(nameof(CARD.CARD_READs))]
    public virtual CARD Card { get; set; } = null!;
}