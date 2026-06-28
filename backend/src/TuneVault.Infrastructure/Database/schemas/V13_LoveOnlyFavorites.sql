USE [TuneVaultDb];

IF COL_LENGTH('dbo.Favorites', 'Reaction') IS NOT NULL
BEGIN
    DECLARE @defaultConstraintName sysname;

    SELECT @defaultConstraintName = dc.name
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.Favorites')
      AND c.name = 'Reaction';

    IF @defaultConstraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.Favorites DROP CONSTRAINT [' + @defaultConstraintName + N']');
    END

    IF EXISTS (
        SELECT 1
        FROM sys.check_constraints
        WHERE name = 'CK_Favorites_Reaction'
          AND parent_object_id = OBJECT_ID('dbo.Favorites')
    )
    BEGIN
        ALTER TABLE dbo.Favorites
        DROP CONSTRAINT CK_Favorites_Reaction;
    END

    ALTER TABLE dbo.Favorites
    DROP COLUMN Reaction;
END
