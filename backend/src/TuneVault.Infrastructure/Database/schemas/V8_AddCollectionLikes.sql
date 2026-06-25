IF OBJECT_ID(N'dbo.CollectionLikes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CollectionLikes
    (
        Id varchar(10) NOT NULL,
        UserId varchar(10) NOT NULL,
        TargetId varchar(10) NOT NULL,
        TargetType tinyint NOT NULL,
        LikedAt datetime2 NOT NULL CONSTRAINT DF_CollectionLikes_LikedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_CollectionLikes PRIMARY KEY (Id),
        CONSTRAINT FK_CollectionLikes_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_CollectionLikes_TargetType CHECK (TargetType IN (1, 2)),
        CONSTRAINT UQ_CollectionLikes_User_Target UNIQUE (UserId, TargetId, TargetType)
    );

    CREATE NONCLUSTERED INDEX IX_CollectionLikes_UserId_LikedAt
        ON dbo.CollectionLikes (UserId ASC, LikedAt DESC);
END;
