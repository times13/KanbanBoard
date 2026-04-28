using System;
using System.Collections.Generic;
using KanbanBoard.AccesDonnee.Models;
using Microsoft.EntityFrameworkCore;

namespace KanbanBoard.AccesDonnee.EFCore;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ACTIVITY_LOG> ACTIVITY_LOGs { get; set; }

    public virtual DbSet<ATTACHMENT> ATTACHMENTs { get; set; }

    public virtual DbSet<BOARD> BOARDs { get; set; }

    public virtual DbSet<BOARD_COLUMN> BOARD_COLUMNs { get; set; }

    public virtual DbSet<BOARD_MEMBER> BOARD_MEMBERs { get; set; }

    public virtual DbSet<CARD> CARDs { get; set; }

    public virtual DbSet<COMMENT> COMMENTs { get; set; }

    public virtual DbSet<LABEL> LABELs { get; set; }

    public virtual DbSet<NOTIFICATION> NOTIFICATIONs { get; set; }

    public virtual DbSet<USER> USERs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ACTIVITY_LOG>(entity =>
        {
            entity.ToTable("ACTIVITY_LOG");

            entity.HasIndex(e => e.BoardId, "IX_ACTIVITY_LOG_BoardId");

            entity.HasIndex(e => e.OccurredAt, "IX_ACTIVITY_LOG_OccurredAt").IsDescending();

            entity.Property(e => e.Action).HasMaxLength(40);
            entity.Property(e => e.EntityType).HasMaxLength(20);
            entity.Property(e => e.OccurredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Board).WithMany(p => p.ACTIVITY_LOGs)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_ACTIVITY_LOG_Board");

            entity.HasOne(d => d.User).WithMany(p => p.ACTIVITY_LOGs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACTIVITY_LOG_User");
        });

        modelBuilder.Entity<ATTACHMENT>(entity =>
        {
            entity.ToTable("ATTACHMENT");

            entity.HasIndex(e => e.CardId, "IX_ATTACHMENT_CardId");

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Card).WithMany(p => p.ATTACHMENTs)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_ATTACHMENT_Card");

            entity.HasOne(d => d.UploadedBy).WithMany(p => p.ATTACHMENTs)
                .HasForeignKey(d => d.UploadedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ATTACHMENT_Uploader");
        });

        modelBuilder.Entity<BOARD>(entity =>
        {
            entity.ToTable("BOARD");

            entity.HasIndex(e => e.OwnerId, "IX_BOARD_OwnerId");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Owner).WithMany(p => p.BOARDs)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOARD_Owner");
        });

        modelBuilder.Entity<BOARD_COLUMN>(entity =>
        {
            entity.ToTable("BOARD_COLUMN");

            entity.HasIndex(e => e.BoardId, "IX_BOARD_COLUMN_BoardId");

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Board).WithMany(p => p.BOARD_COLUMNs)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_BOARD_COLUMN_Board");
        });

        modelBuilder.Entity<BOARD_MEMBER>(entity =>
        {
            entity.HasKey(e => new { e.BoardId, e.UserId });

            entity.ToTable("BOARD_MEMBER");

            entity.HasIndex(e => e.UserId, "IX_BOARD_MEMBER_UserId");

            entity.Property(e => e.JoinedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Member");

            entity.HasOne(d => d.Board).WithMany(p => p.BOARD_MEMBERs)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_BOARD_MEMBER_Board");

            entity.HasOne(d => d.User).WithMany(p => p.BOARD_MEMBERs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BOARD_MEMBER_User");
        });

        modelBuilder.Entity<CARD>(entity =>
        {
            entity.ToTable("CARD");

            entity.HasIndex(e => e.AssigneeId, "IX_CARD_AssigneeId").HasFilter("([AssigneeId] IS NOT NULL)");

            entity.HasIndex(e => e.ColumnId, "IX_CARD_ColumnId");

            entity.HasIndex(e => e.IsArchived, "IX_CARD_IsArchived");

            entity.Property(e => e.ArchivedAt).HasPrecision(0);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.DueDate).HasPrecision(0);
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.ArchivedBy).WithMany(p => p.CARDArchivedBies)
                .HasForeignKey(d => d.ArchivedById)
                .HasConstraintName("FK_CARD_ArchivedBy");

            entity.HasOne(d => d.Assignee).WithMany(p => p.CARDAssignees)
                .HasForeignKey(d => d.AssigneeId)
                .HasConstraintName("FK_CARD_Assignee");

            entity.HasOne(d => d.Column).WithMany(p => p.CARDs)
                .HasForeignKey(d => d.ColumnId)
                .HasConstraintName("FK_CARD_Column");

            entity.HasMany(d => d.Labels).WithMany(p => p.Cards)
                .UsingEntity<Dictionary<string, object>>(
                    "CARD_LABEL",
                    r => r.HasOne<LABEL>().WithMany()
                        .HasForeignKey("LabelId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CARD_LABEL_Label"),
                    l => l.HasOne<CARD>().WithMany()
                        .HasForeignKey("CardId")
                        .HasConstraintName("FK_CARD_LABEL_Card"),
                    j =>
                    {
                        j.HasKey("CardId", "LabelId");
                        j.ToTable("CARD_LABEL");
                        j.HasIndex(new[] { "LabelId" }, "IX_CARD_LABEL_LabelId");
                    });
        });

        modelBuilder.Entity<COMMENT>(entity =>
        {
            entity.ToTable("COMMENT");

            entity.HasIndex(e => e.AuthorId, "IX_COMMENT_AuthorId");

            entity.HasIndex(e => e.CardId, "IX_COMMENT_CardId");

            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Author).WithMany(p => p.COMMENTs)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COMMENT_Author");

            entity.HasOne(d => d.Card).WithMany(p => p.COMMENTs)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_COMMENT_Card");
        });

        modelBuilder.Entity<LABEL>(entity =>
        {
            entity.ToTable("LABEL");

            entity.HasIndex(e => e.BoardId, "IX_LABEL_BoardId");

            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#808080");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Board).WithMany(p => p.LABELs)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_LABEL_Board");
        });

        modelBuilder.Entity<NOTIFICATION>(entity =>
        {
            entity.ToTable("NOTIFICATION");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_NOTIFICATION_User_IsRead").IsDescending(false, false, true);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.Actor).WithMany(p => p.NOTIFICATIONActors)
                .HasForeignKey(d => d.ActorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NOTIFICATION_Actor");

            entity.HasOne(d => d.Board).WithMany(p => p.NOTIFICATIONs)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK_NOTIFICATION_Board");

            entity.HasOne(d => d.Card).WithMany(p => p.NOTIFICATIONs)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_NOTIFICATION_Card");

            entity.HasOne(d => d.User).WithMany(p => p.NOTIFICATIONUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NOTIFICATION_User");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.ToTable("USER");

            entity.HasIndex(e => e.Email, "UQ_USER_Email").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(60);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
