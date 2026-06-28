-- V4: Map Entity ValueObjects to Database Columns
-- Purpose: Add columns to match Entity.Url and Duration value objects.
-- Migration: AudioUrl/VideoUrl → Url, DurationSeconds → DurationMinutes.

USE [TuneVaultDb];

-- ============================================================================
-- Step 1: Add new columns to MediaItems TABLE FIRST (before any UPDATE/SELECT)
-- ============================================================================

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'MediaItems' AND COLUMN_NAME = 'Url')
BEGIN
    ALTER TABLE [MediaItems] ADD Url varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;
    PRINT 'Column Url added successfully.';
END
ELSE
BEGIN
    PRINT 'Column Url already exists.';
END;

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'MediaItems' AND COLUMN_NAME = 'DurationMinutes')
BEGIN
    ALTER TABLE [MediaItems] ADD DurationMinutes int DEFAULT 0 NOT NULL;
    PRINT 'Column DurationMinutes added successfully.';
END
ELSE
BEGIN
    PRINT 'Column DurationMinutes already exists.';
END;

-- ============================================================================
-- Step 2: Migrate data from AudioUrl/VideoUrl to Url (if not already done)
-- ============================================================================

PRINT 'Starting data migration for Url column...';

-- First, prioritize AudioUrl, then VideoUrl, for existing records
UPDATE [MediaItems]
SET Url = COALESCE(AudioUrl, VideoUrl, 'https://placeholder.tunevault.com')
WHERE Url IS NULL;

PRINT 'Url column migration completed.';

-- ============================================================================
-- Step 3: Migrate DurationSeconds to DurationMinutes (only if needed)
-- ============================================================================

PRINT 'Starting data migration for DurationMinutes column...';

-- Only update if DurationMinutes is still 0 and DurationSeconds has data
UPDATE [MediaItems]
SET DurationMinutes = DurationSeconds / 60
WHERE DurationMinutes = 0 AND DurationSeconds > 0;

PRINT 'DurationMinutes migration completed.';

-- ============================================================================
-- Step 4: Set Url as NOT NULL after migration (ensure no NULLs remain)
-- ============================================================================

PRINT 'Making Url column NOT NULL...';

ALTER TABLE [MediaItems] ALTER COLUMN Url varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;

PRINT 'Url column is now NOT NULL.';

-- ============================================================================
-- Step 5: Create index on Url (for optimization if needed later)
-- ============================================================================

IF NOT EXISTS (SELECT 1
FROM sys.indexes
WHERE name = 'IX_MediaItems_Url' AND object_id = OBJECT_ID('MediaItems'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_MediaItems_Url ON TuneVaultDb.dbo.MediaItems (Url ASC)
        WITH (PAD_INDEX = OFF, FILLFACTOR = 100, SORT_IN_TEMPDB = OFF, 
              IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, 
              ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT 'Index IX_MediaItems_Url created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_MediaItems_Url already exists.';
END;

PRINT '=============================================================';
PRINT 'V4 Migration completed successfully!';
PRINT '=============================================================';
PRINT 'New columns added: Url, DurationMinutes';
PRINT 'Old columns retained: AudioUrl, VideoUrl, DurationSeconds (for backward compatibility)';
PRINT 'Next step: Update application code to use new columns';
PRINT '=============================================================';
