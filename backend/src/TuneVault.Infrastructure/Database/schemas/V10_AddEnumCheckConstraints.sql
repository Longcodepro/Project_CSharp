USE TuneVault;

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_MediaItems_MediaType'
      AND parent_object_id = OBJECT_ID('dbo.MediaItems')
)
BEGIN
    ALTER TABLE dbo.MediaItems
    ADD CONSTRAINT CK_MediaItems_MediaType CHECK (MediaType IN (0, 1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_MediaItems_AccessLevel'
      AND parent_object_id = OBJECT_ID('dbo.MediaItems')
)
BEGIN
    ALTER TABLE dbo.MediaItems
    ADD CONSTRAINT CK_MediaItems_AccessLevel CHECK (AccessLevel IN (0, 1));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Albums_ContentType'
      AND parent_object_id = OBJECT_ID('dbo.Albums')
)
BEGIN
    ALTER TABLE dbo.Albums
    ADD CONSTRAINT CK_Albums_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Playlists_ContentType'
      AND parent_object_id = OBJECT_ID('dbo.Playlists')
)
BEGIN
    ALTER TABLE dbo.Playlists
    ADD CONSTRAINT CK_Playlists_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Friends_Status'
      AND parent_object_id = OBJECT_ID('dbo.Friends')
)
BEGIN
    ALTER TABLE dbo.Friends
    ADD CONSTRAINT CK_Friends_Status CHECK (Status IN (1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_MediaShares_ShareType'
      AND parent_object_id = OBJECT_ID('dbo.MediaShares')
)
BEGIN
    ALTER TABLE dbo.MediaShares
    ADD CONSTRAINT CK_MediaShares_ShareType CHECK (ShareType IN (1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Notifications_NotifyType'
      AND parent_object_id = OBJECT_ID('dbo.Notifications')
)
BEGIN
    ALTER TABLE dbo.Notifications
    ADD CONSTRAINT CK_Notifications_NotifyType CHECK (NotifyType IN (1, 2, 3, 4, 5, 6));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Notifications_TargetType'
      AND parent_object_id = OBJECT_ID('dbo.Notifications')
)
BEGIN
    ALTER TABLE dbo.Notifications
    ADD CONSTRAINT CK_Notifications_TargetType CHECK (TargetType IS NULL OR TargetType IN (1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Ads_AdType'
      AND parent_object_id = OBJECT_ID('dbo.Ads')
)
BEGIN
    ALTER TABLE dbo.Ads
    ADD CONSTRAINT CK_Ads_AdType CHECK (AdType IN (1, 2, 3));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_OtpLogs_Purpose'
      AND parent_object_id = OBJECT_ID('dbo.OtpLogs')
)
BEGIN
    ALTER TABLE dbo.OtpLogs
    ADD CONSTRAINT CK_OtpLogs_Purpose CHECK (Purpose IN ('register', 'reset_password', 'change_password'));
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Favorites_Reaction'
      AND parent_object_id = OBJECT_ID('dbo.Favorites')
)
BEGIN
    ALTER TABLE dbo.Favorites
    ADD CONSTRAINT CK_Favorites_Reaction CHECK (Reaction IN (0, 1, 2, 3, 4, 5));
END
