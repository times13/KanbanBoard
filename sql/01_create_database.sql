-- =====================================================================
--  KanbanBoard — Script de création de la base SQL Server
--  MBDS 2025-2026 — PDR 7
--  Cible  : SQL Server 2019+ (LocalDB / Express / Developer)
-- =====================================================================

-- 0. Création de la base (à exécuter en mode "master" dans SSMS)
-- ---------------------------------------------------------------------
IF DB_ID(N'KanbanBoardDb') IS NULL
BEGIN
    CREATE DATABASE KanbanBoardDb;
END
GO

USE KanbanBoardDb;
GO

-- ---------------------------------------------------------------------
-- 1. Suppression des tables existantes (ordre inverse des FK)
--    Utile si on relance le script plusieurs fois en dev.
-- ---------------------------------------------------------------------
IF OBJECT_ID(N'dbo.NOTIFICATION', N'U') IS NOT NULL DROP TABLE dbo.NOTIFICATION;
IF OBJECT_ID(N'dbo.ACTIVITY_LOG', N'U') IS NOT NULL DROP TABLE dbo.ACTIVITY_LOG;
IF OBJECT_ID(N'dbo.CARD_LABEL',   N'U') IS NOT NULL DROP TABLE dbo.CARD_LABEL;
IF OBJECT_ID(N'dbo.LABEL',        N'U') IS NOT NULL DROP TABLE dbo.LABEL;
IF OBJECT_ID(N'dbo.ATTACHMENT',   N'U') IS NOT NULL DROP TABLE dbo.ATTACHMENT;
IF OBJECT_ID(N'dbo.COMMENT',      N'U') IS NOT NULL DROP TABLE dbo.COMMENT;
IF OBJECT_ID(N'dbo.CARD',         N'U') IS NOT NULL DROP TABLE dbo.CARD;
IF OBJECT_ID(N'dbo.BOARD_COLUMN', N'U') IS NOT NULL DROP TABLE dbo.BOARD_COLUMN;
IF OBJECT_ID(N'dbo.BOARD_MEMBER', N'U') IS NOT NULL DROP TABLE dbo.BOARD_MEMBER;
IF OBJECT_ID(N'dbo.BOARD',        N'U') IS NOT NULL DROP TABLE dbo.BOARD;
IF OBJECT_ID(N'dbo.[USER]',       N'U') IS NOT NULL DROP TABLE dbo.[USER];
GO

-- =====================================================================
-- 2. Tables de base (sans dépendances)
-- =====================================================================

-- ---------- USER ----------
CREATE TABLE dbo.[USER] (
    Id            INT             IDENTITY(1,1) NOT NULL,
    Username      NVARCHAR(100)   NOT NULL,
    Email         NVARCHAR(255)   NOT NULL,
    PasswordHash  NVARCHAR(60)    NOT NULL,             -- BCrypt = 60 chars
    IsGlobalAdmin BIT             NOT NULL DEFAULT (0),
    CreatedAt     DATETIME2(0)    NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_USER PRIMARY KEY (Id),
    CONSTRAINT UQ_USER_Email UNIQUE (Email)
);
GO

-- =====================================================================
-- 3. Tables niveau 1 (dépendent de USER)
-- =====================================================================

-- ---------- BOARD ----------
CREATE TABLE dbo.BOARD (
    Id          INT            IDENTITY(1,1) NOT NULL,
    OwnerId     INT            NOT NULL,
    Title       NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(2000) NULL,
    CreatedAt   DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(0)   NULL,
    CONSTRAINT PK_BOARD PRIMARY KEY (Id),
    CONSTRAINT FK_BOARD_Owner
        FOREIGN KEY (OwnerId) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION
);
GO

CREATE INDEX IX_BOARD_OwnerId ON dbo.BOARD(OwnerId);
GO

-- =====================================================================
-- 4. Tables niveau 2 (dépendent de BOARD et USER)
-- =====================================================================

-- ---------- BOARD_MEMBER ----------
CREATE TABLE dbo.BOARD_MEMBER (
    BoardId  INT          NOT NULL,
    UserId   INT          NOT NULL,
    Role     NVARCHAR(20) NOT NULL DEFAULT ('Member'),
    JoinedAt DATETIME2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_BOARD_MEMBER PRIMARY KEY (BoardId, UserId),
    CONSTRAINT FK_BOARD_MEMBER_Board
        FOREIGN KEY (BoardId) REFERENCES dbo.BOARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_BOARD_MEMBER_User
        FOREIGN KEY (UserId)  REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,                              -- évite multi-cascade SQL Server
    CONSTRAINT CK_BOARD_MEMBER_Role
        CHECK (Role IN (N'Admin', N'Member', N'Viewer'))
);
GO

CREATE INDEX IX_BOARD_MEMBER_UserId ON dbo.BOARD_MEMBER(UserId);
GO

-- ---------- BOARD_COLUMN ----------
CREATE TABLE dbo.BOARD_COLUMN (
    Id         INT           IDENTITY(1,1) NOT NULL,
    BoardId    INT           NOT NULL,
    Title      NVARCHAR(100) NOT NULL,
    Position   INT           NOT NULL,
    RowVersion ROWVERSION    NOT NULL,                    -- concurrence optimiste
    CONSTRAINT PK_BOARD_COLUMN PRIMARY KEY (Id),
    CONSTRAINT FK_BOARD_COLUMN_Board
        FOREIGN KEY (BoardId) REFERENCES dbo.BOARD(Id)
        ON DELETE CASCADE
);
GO

CREATE INDEX IX_BOARD_COLUMN_BoardId ON dbo.BOARD_COLUMN(BoardId);
GO

-- ---------- LABEL ----------
CREATE TABLE dbo.LABEL (
    Id      INT          IDENTITY(1,1) NOT NULL,
    BoardId INT          NOT NULL,
    Name    NVARCHAR(50) NOT NULL,
    Color   NVARCHAR(7)  NOT NULL DEFAULT (N'#808080'),   -- ex: #FF5733
    CONSTRAINT PK_LABEL PRIMARY KEY (Id),
    CONSTRAINT FK_LABEL_Board
        FOREIGN KEY (BoardId) REFERENCES dbo.BOARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT CK_LABEL_Color
        CHECK (Color LIKE N'#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);
GO

CREATE INDEX IX_LABEL_BoardId ON dbo.LABEL(BoardId);
GO

-- =====================================================================
-- 5. Tables niveau 3 (dépendent de BOARD_COLUMN)
-- =====================================================================

-- ---------- CARD ----------
CREATE TABLE dbo.CARD (
    Id           INT            IDENTITY(1,1) NOT NULL,
    ColumnId     INT            NOT NULL,
    AssigneeId   INT            NULL,
    Title        NVARCHAR(200)  NOT NULL,
    Description  NVARCHAR(4000) NULL,
    Priority     NVARCHAR(20)   NOT NULL DEFAULT ('Medium'),
    Position     INT            NOT NULL,
    DueDate      DATETIME2(0)   NULL,
    CreatedAt    DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt    DATETIME2(0)   NULL,
    RowVersion   ROWVERSION     NOT NULL,                  -- concurrence optimiste
    IsArchived   BIT            NOT NULL DEFAULT (0),
    ArchivedAt   DATETIME2(0)   NULL,
    ArchivedById INT            NULL,
    CONSTRAINT PK_CARD PRIMARY KEY (Id),
    CONSTRAINT FK_CARD_Column
        FOREIGN KEY (ColumnId) REFERENCES dbo.BOARD_COLUMN(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_CARD_Assignee
        FOREIGN KEY (AssigneeId) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_CARD_ArchivedBy
        FOREIGN KEY (ArchivedById) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_CARD_Priority
        CHECK (Priority IN (N'Low', N'Medium', N'High', N'Critical'))
);
GO

CREATE INDEX IX_CARD_ColumnId   ON dbo.CARD(ColumnId);
CREATE INDEX IX_CARD_AssigneeId ON dbo.CARD(AssigneeId) WHERE AssigneeId IS NOT NULL;
CREATE INDEX IX_CARD_IsArchived ON dbo.CARD(IsArchived);
GO

-- =====================================================================
-- 6. Tables niveau 4 (dépendent de CARD)
-- =====================================================================

-- ---------- COMMENT ----------
CREATE TABLE dbo.[COMMENT] (
    Id        INT            IDENTITY(1,1) NOT NULL,
    CardId    INT            NOT NULL,
    AuthorId  INT            NOT NULL,
    Content   NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_COMMENT PRIMARY KEY (Id),
    CONSTRAINT FK_COMMENT_Card
        FOREIGN KEY (CardId) REFERENCES dbo.CARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_COMMENT_Author
        FOREIGN KEY (AuthorId) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION
);
GO

CREATE INDEX IX_COMMENT_CardId   ON dbo.[COMMENT](CardId);
CREATE INDEX IX_COMMENT_AuthorId ON dbo.[COMMENT](AuthorId);
GO

-- ---------- ATTACHMENT ----------
CREATE TABLE dbo.ATTACHMENT (
    Id           INT           IDENTITY(1,1) NOT NULL,
    CardId       INT           NOT NULL,
    UploadedById INT           NOT NULL,
    FileName     NVARCHAR(255) NOT NULL,
    FileUrl      NVARCHAR(500) NOT NULL,
    FileSizeKB   BIGINT        NULL,
    UploadedAt   DATETIME2(0)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ATTACHMENT PRIMARY KEY (Id),
    CONSTRAINT FK_ATTACHMENT_Card
        FOREIGN KEY (CardId) REFERENCES dbo.CARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_ATTACHMENT_Uploader
        FOREIGN KEY (UploadedById) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION
);
GO

CREATE INDEX IX_ATTACHMENT_CardId ON dbo.ATTACHMENT(CardId);
GO

-- ---------- CARD_LABEL ----------
CREATE TABLE dbo.CARD_LABEL (
    CardId  INT NOT NULL,
    LabelId INT NOT NULL,
    CONSTRAINT PK_CARD_LABEL PRIMARY KEY (CardId, LabelId),
    CONSTRAINT FK_CARD_LABEL_Card
        FOREIGN KEY (CardId)  REFERENCES dbo.CARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_CARD_LABEL_Label
        FOREIGN KEY (LabelId) REFERENCES dbo.LABEL(Id)
        ON DELETE NO ACTION                                -- évite multi-cascade
);
GO

CREATE INDEX IX_CARD_LABEL_LabelId ON dbo.CARD_LABEL(LabelId);
GO

-- =====================================================================
-- 7. Tables transverses (logs et notifications)
-- =====================================================================

-- ---------- ACTIVITY_LOG ----------
CREATE TABLE dbo.ACTIVITY_LOG (
    Id         INT          IDENTITY(1,1) NOT NULL,
    BoardId    INT          NOT NULL,
    UserId     INT          NOT NULL,
    EntityType NVARCHAR(20) NOT NULL,
    EntityId   INT          NULL,
    Action     NVARCHAR(40) NOT NULL,
    OccurredAt DATETIME2(0) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ACTIVITY_LOG PRIMARY KEY (Id),
    CONSTRAINT FK_ACTIVITY_LOG_Board
        FOREIGN KEY (BoardId) REFERENCES dbo.BOARD(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_ACTIVITY_LOG_User
        FOREIGN KEY (UserId)  REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_ACTIVITY_LOG_EntityType
        CHECK (EntityType IN (
            N'Card', N'Column', N'Board', N'Member',
            N'Label', N'Attachment', N'Comment'
        )),
    CONSTRAINT CK_ACTIVITY_LOG_Action
        CHECK (Action IN (
            N'CardCreated', N'CardMoved', N'CardUpdated', N'CardArchived',
            N'CardUnarchived', N'CardDeleted',
            N'ColumnCreated', N'ColumnRenamed', N'ColumnMoved', N'ColumnDeleted',
            N'MemberAdded', N'MemberRemoved', N'MemberRoleChanged',
            N'CommentAdded', N'CommentDeleted',
            N'LabelCreated', N'LabelAddedToCard',
            N'LabelRemovedFromCard', N'LabelDeleted',
            N'AttachmentUploaded', N'AttachmentDeleted',
            N'BoardCreated', N'BoardUpdated'
        ))
);
GO

CREATE INDEX IX_ACTIVITY_LOG_BoardId    ON dbo.ACTIVITY_LOG(BoardId);
CREATE INDEX IX_ACTIVITY_LOG_OccurredAt ON dbo.ACTIVITY_LOG(OccurredAt DESC);
GO

-- ---------- NOTIFICATION ----------
CREATE TABLE dbo.NOTIFICATION (
    Id        INT           IDENTITY(1,1) NOT NULL,
    UserId    INT           NOT NULL,                -- destinataire
    ActorId   INT           NOT NULL,                -- déclencheur
    CardId    INT           NULL,
    BoardId   INT           NULL,
    [Type]    NVARCHAR(30)  NOT NULL,
    Message   NVARCHAR(500) NOT NULL,
    IsRead    BIT           NOT NULL DEFAULT (0),
    CreatedAt DATETIME2(0)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_NOTIFICATION PRIMARY KEY (Id),
    CONSTRAINT FK_NOTIFICATION_User
        FOREIGN KEY (UserId)  REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_NOTIFICATION_Actor
        FOREIGN KEY (ActorId) REFERENCES dbo.[USER](Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_NOTIFICATION_Card
        FOREIGN KEY (CardId)  REFERENCES dbo.CARD(Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_NOTIFICATION_Board
        FOREIGN KEY (BoardId) REFERENCES dbo.BOARD(Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_NOTIFICATION_Type
        CHECK ([Type] IN (
            N'CardAssigned', N'CardUnassigned', N'Commented',
            N'Mentioned', N'CardMoved',
            N'DueSoon', N'DueToday', N'Overdue', N'MemberAdded'
        ))
);
GO

CREATE INDEX IX_NOTIFICATION_User_IsRead
    ON dbo.NOTIFICATION(UserId, IsRead, CreatedAt DESC);
GO

PRINT N'Base KanbanBoardDb créée avec succès.';
GO
