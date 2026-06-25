IF COL_LENGTH('dbo.Friends', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Friends
    ADD IsActive bit NOT NULL
        CONSTRAINT DF_Friends_IsActive DEFAULT 1;
END;
