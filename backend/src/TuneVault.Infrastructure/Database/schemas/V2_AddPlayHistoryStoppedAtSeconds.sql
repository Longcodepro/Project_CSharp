IF COL_LENGTH('dbo.PlayHistory', 'StoppedAtSeconds') IS NULL
BEGIN
    ALTER TABLE dbo.PlayHistory
    ADD StoppedAtSeconds int NULL;
END
