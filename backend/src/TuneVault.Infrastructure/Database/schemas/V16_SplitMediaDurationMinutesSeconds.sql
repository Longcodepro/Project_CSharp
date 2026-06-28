USE [TuneVaultDb];
GO

SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.MediaItems', N'DurationMinutes') IS NULL
BEGIN
    ALTER TABLE dbo.MediaItems
    ADD DurationMinutes int NULL;
END
GO

UPDATE dbo.MediaItems
SET
    DurationMinutes = CASE
        WHEN ISNULL(DurationSeconds, 0) >= 60
            THEN ISNULL(DurationSeconds, 0) / 60
        ELSE ISNULL(DurationMinutes, 0)
    END,
    DurationSeconds = CASE
        WHEN ISNULL(DurationSeconds, 0) >= 60
            THEN ISNULL(DurationSeconds, 0) % 60
        ELSE ISNULL(DurationSeconds, 0)
    END
WHERE DurationMinutes IS NULL
   OR DurationSeconds IS NULL
   OR DurationSeconds >= 60;
GO

UPDATE dbo.MediaItems
SET DurationMinutes = ISNULL(DurationMinutes, 0),
    DurationSeconds = ISNULL(DurationSeconds, 0)
WHERE DurationMinutes IS NULL
   OR DurationSeconds IS NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_MediaItems_DurationParts'
      AND parent_object_id = OBJECT_ID(N'dbo.MediaItems')
)
BEGIN
    ALTER TABLE dbo.MediaItems DROP CONSTRAINT CK_MediaItems_DurationParts;
END
GO

DECLARE @durationMinutesNullable bit = 0;
DECLARE @durationSecondsNullable bit = 0;

SELECT @durationMinutesNullable = c.is_nullable
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.MediaItems')
  AND c.name = N'DurationMinutes';

SELECT @durationSecondsNullable = c.is_nullable
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.MediaItems')
  AND c.name = N'DurationSeconds';

IF @durationMinutesNullable = 1
BEGIN
    ALTER TABLE dbo.MediaItems ALTER COLUMN DurationMinutes int NOT NULL;
END

IF @durationSecondsNullable = 1
BEGIN
    ALTER TABLE dbo.MediaItems ALTER COLUMN DurationSeconds int NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_MediaItems_DurationParts'
      AND parent_object_id = OBJECT_ID(N'dbo.MediaItems')
)
BEGIN
    ALTER TABLE dbo.MediaItems
    ADD CONSTRAINT CK_MediaItems_DurationParts
    CHECK (DurationMinutes >= 0 AND DurationSeconds BETWEEN 0 AND 59);
END
GO
