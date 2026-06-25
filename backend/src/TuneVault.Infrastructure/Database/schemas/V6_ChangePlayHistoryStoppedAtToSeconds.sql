IF OBJECT_ID(N'dbo.PlayHistory', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.PlayHistory', N'StoppedAt') IS NULL
    BEGIN
        ALTER TABLE dbo.PlayHistory
        ADD StoppedAt int NULL;
    END
    ELSE IF EXISTS
    (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.PlayHistory')
          AND c.name = N'StoppedAt'
          AND t.name <> N'int'
    )
    BEGIN
        ALTER TABLE dbo.PlayHistory
        ADD StoppedAt_New int NULL;

        IF COL_LENGTH(N'dbo.PlayHistory', N'StoppedAtSeconds') IS NOT NULL
        BEGIN
            EXEC(N'
                UPDATE dbo.PlayHistory
                SET StoppedAt_New = StoppedAtSeconds
                WHERE StoppedAtSeconds IS NOT NULL;
            ');
        END

        ALTER TABLE dbo.PlayHistory
        DROP COLUMN StoppedAt;

        EXEC sp_rename
            N'dbo.PlayHistory.StoppedAt_New',
            N'StoppedAt',
            N'COLUMN';
    END

    IF COL_LENGTH(N'dbo.PlayHistory', N'StoppedAtSeconds') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.PlayHistory
        DROP COLUMN StoppedAtSeconds;
    END
END
