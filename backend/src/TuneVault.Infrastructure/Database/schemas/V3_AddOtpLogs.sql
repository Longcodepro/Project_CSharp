USE TuneVault;
GO

CREATE TABLE OtpLogs
(
    Id varchar(10) NOT NULL,
    Email varchar(255) NOT NULL,
    OtpCode varchar(10) NOT NULL,
    Purpose varchar(20) NOT NULL,
    -- 'register' | 'reset_password'
    CreatedAt datetime2 NOT NULL CONSTRAINT DF_OtpLogs_CreatedAt DEFAULT GETUTCDATE(),
    ExpiresAt datetime2 NOT NULL,
    -- = CreatedAt + 1 phút 30 giây
    IsActive bit NOT NULL CONSTRAINT DF_OtpLogs_IsActive DEFAULT 1,
    CONSTRAINT PK_OtpLogs PRIMARY KEY (Id)
);
GO

CREATE INDEX IX_OtpLogs_Email_Purpose_IsActive ON OtpLogs (Email, Purpose, IsActive);
GO
