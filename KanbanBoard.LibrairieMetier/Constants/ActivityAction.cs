namespace KanbanBoard.LibrairieMetier.Constants;

public static class ActivityAction
{
    // Cartes
    public const string CardCreated = "CardCreated";
    public const string CardMoved = "CardMoved";
    public const string CardUpdated = "CardUpdated";
    public const string CardArchived = "CardArchived";
    public const string CardUnarchived = "CardUnarchived";
    public const string CardDeleted = "CardDeleted";

    // Colonnes
    public const string ColumnCreated = "ColumnCreated";
    public const string ColumnRenamed = "ColumnRenamed";
    public const string ColumnMoved = "ColumnMoved";
    public const string ColumnDeleted = "ColumnDeleted";

    // Membres
    public const string MemberAdded = "MemberAdded";
    public const string MemberRemoved = "MemberRemoved";
    public const string MemberRoleChanged = "MemberRoleChanged";

    // Commentaires
    public const string CommentAdded = "CommentAdded";
    public const string CommentDeleted = "CommentDeleted";

    // Labels (pas implémentés mais réservés)
    public const string LabelCreated = "LabelCreated";
    public const string LabelAddedToCard = "LabelAddedToCard";
    public const string LabelRemovedFromCard = "LabelRemovedFromCard";
    public const string LabelDeleted = "LabelDeleted";

    // Attachements (NB: AttachmentUploaded, pas AttachmentAdded !)
    public const string AttachmentUploaded = "AttachmentUploaded";
    public const string AttachmentDeleted = "AttachmentDeleted";

    // Board
    public const string BoardCreated = "BoardCreated";
    public const string BoardUpdated = "BoardUpdated";
}