-- DROP SCHEMA dbo;

CREATE SCHEMA dbo;
-- TuneVault.dbo.AccountTiers definition

-- Drop table

-- DROP TABLE TuneVault.dbo.AccountTiers;

CREATE TABLE TuneVault.dbo.AccountTiers (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Code varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Name nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PriceAmount decimal(10,2) NOT NULL,
	PriceCurrency varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT 'USD' NOT NULL,
	MaxUploadMb int NOT NULL,
	CanDownload bit DEFAULT 0 NOT NULL,
	CanSkipAds bit DEFAULT 0 NOT NULL,
	MaxDevices int DEFAULT 1 NOT NULL,
	DurationInDays int NOT NULL,
	CreatedAt datetime2 NOT NULL,
	ActiveFrom datetime2 NOT NULL,
	ActiveTo datetime2 NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__AccountT__3214EC07646FBE11 PRIMARY KEY (Id),
	CONSTRAINT UQ__AccountT__A25C5AA71DA9CC75 UNIQUE (Code)
);


-- TuneVault.dbo.Admins definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Admins;

CREATE TABLE TuneVault.dbo.Admins (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Name nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Email varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PasswordHash varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PhoneNumber varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Role] varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__Admins__3214EC07A436EA86 PRIMARY KEY (Id),
	CONSTRAINT UQ__Admins__A9D10534A895CC10 UNIQUE (Email)
);


-- TuneVault.dbo.Ads definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Ads;

CREATE TABLE TuneVault.dbo.Ads (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Title nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Advertiser nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	AdType tinyint NOT NULL,
	MediaUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	ClickThroughUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	DurationSeconds int NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CreatedAt datetime2 NOT NULL,
	CONSTRAINT PK__Ads__3214EC0798C30D80 PRIMARY KEY (Id)
);


-- TuneVault.dbo.OtpLogs definition

-- Drop table

-- DROP TABLE TuneVault.dbo.OtpLogs;

CREATE TABLE TuneVault.dbo.OtpLogs (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Email varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	OtpCode varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Purpose varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CreatedAt datetime2 DEFAULT getutcdate() NOT NULL,
	ExpiresAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK_OtpLogs PRIMARY KEY (Id)
);
 CREATE NONCLUSTERED INDEX IX_OtpLogs_Email_Purpose_IsActive ON TuneVault.dbo.OtpLogs (  Email ASC  , Purpose ASC  , IsActive ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- TuneVault.dbo.Users definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Users;

CREATE TABLE TuneVault.dbo.Users (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	IdDisplay varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	DisplayName nvarchar(150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Email varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PasswordHash varchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	AvatarUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Bio nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsArtist bit DEFAULT 0 NOT NULL,
	TotalFollowers int DEFAULT 0 NOT NULL,
	CreatedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__Users__3214EC0727F2F97F PRIMARY KEY (Id),
	CONSTRAINT UQ__Users__A9D1053481C47493 UNIQUE (Email),
	CONSTRAINT UQ__Users__BEDCC22A18BF3D40 UNIQUE (IdDisplay)
);


-- TuneVault.dbo.Albums definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Albums;

CREATE TABLE TuneVault.dbo.Albums (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ArtistId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Title nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Description nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CoverImageUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CreatedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	IsPublic bit DEFAULT 1 NOT NULL,
	ReleaseDate datetime2 NULL,
	ContentType tinyint NULL,
	CONSTRAINT PK__Albums__3214EC07685C75A6 PRIMARY KEY (Id),
	CONSTRAINT FK__Albums__ArtistId__6E01572D FOREIGN KEY (ArtistId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.Follows definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Follows;

CREATE TABLE TuneVault.dbo.Follows (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	FollowerId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	FolloweeId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	FollowedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__Follows__3214EC07CB894B37 PRIMARY KEY (Id),
	CONSTRAINT FK__Follows__Followe__01142BA1 FOREIGN KEY (FollowerId) REFERENCES TuneVault.dbo.Users(Id),
	CONSTRAINT FK__Follows__Followe__02084FDA FOREIGN KEY (FolloweeId) REFERENCES TuneVault.dbo.Users(Id)
);
 CREATE NONCLUSTERED INDEX IX_Follows_FolloweeId ON TuneVault.dbo.Follows (  FolloweeId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- TuneVault.dbo.Friends definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Friends;

CREATE TABLE TuneVault.dbo.Friends (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	RequestedById varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	RequestedToId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Status tinyint DEFAULT 1 NOT NULL,
	CreatedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__Friends__3214EC078FA7D32C PRIMARY KEY (Id),
	CONSTRAINT FK__Friends__Request__05D8E0BE FOREIGN KEY (RequestedById) REFERENCES TuneVault.dbo.Users(Id),
	CONSTRAINT FK__Friends__Request__06CD04F7 FOREIGN KEY (RequestedToId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.MediaItems definition

-- Drop table

-- DROP TABLE TuneVault.dbo.MediaItems;

CREATE TABLE TuneVault.dbo.MediaItems (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	OwnerId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Title nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Description nvarchar(1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	MediaType tinyint NOT NULL,
	AudioUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	VideoUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CoverImageUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CanvasUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Genre nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	DurationSeconds int DEFAULT 0 NOT NULL,
	TrailerSeconds int DEFAULT 0 NOT NULL,
	AccessLevel tinyint DEFAULT 0 NOT NULL,
	IsPublic bit DEFAULT 1 NOT NULL,
	FavoriteCount int DEFAULT 0 NOT NULL,
	ViewCount int DEFAULT 0 NOT NULL,
	UploadedAt datetime2 NOT NULL,
	ReleaseDate datetime2 NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	Url varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	DurationMinutes int DEFAULT 0 NOT NULL,
	TrailerMinutes int DEFAULT 0 NOT NULL,
	IsValid bit DEFAULT 0 NOT NULL,
	CONSTRAINT PK__MediaIte__3214EC0789C51905 PRIMARY KEY (Id),
	CONSTRAINT FK__MediaItem__Owner__60A75C0F FOREIGN KEY (OwnerId) REFERENCES TuneVault.dbo.Users(Id)
);
 CREATE NONCLUSTERED INDEX IX_MediaItems_Genre ON TuneVault.dbo.MediaItems (  Genre ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_MediaItems_OwnerId ON TuneVault.dbo.MediaItems (  OwnerId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_MediaItems_Url ON TuneVault.dbo.MediaItems (  Url ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- TuneVault.dbo.MediaShares definition

-- Drop table

-- DROP TABLE TuneVault.dbo.MediaShares;

CREATE TABLE TuneVault.dbo.MediaShares (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	SenderId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ReceiverId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	SharedItemId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ShareType tinyint NOT NULL,
	Message nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	SharedAt datetime2 NOT NULL,
	CONSTRAINT PK__MediaSha__3214EC079D9959F1 PRIMARY KEY (Id),
	CONSTRAINT FK__MediaShar__Recei__0B91BA14 FOREIGN KEY (ReceiverId) REFERENCES TuneVault.dbo.Users(Id),
	CONSTRAINT FK__MediaShar__Sende__0A9D95DB FOREIGN KEY (SenderId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.Notifications definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Notifications;

CREATE TABLE TuneVault.dbo.Notifications (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	NotifyType tinyint NOT NULL,
	Title nvarchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Message nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	IsRead bit DEFAULT 0 NOT NULL,
	CreatedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	SenderId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	TargetType tinyint NULL,
	TargetId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK__Notifica__3214EC07CD589234 PRIMARY KEY (Id),
	CONSTRAINT FK__Notificat__UserI__0E6E26BF FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.PlayHistory definition

-- Drop table

-- DROP TABLE TuneVault.dbo.PlayHistory;

CREATE TABLE TuneVault.dbo.PlayHistory (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	MediaItemId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	HistoryOrder int NOT NULL,
	StoppedAt int NULL,
	CONSTRAINT PK__PlayHist__3214EC076FBC4541 PRIMARY KEY (Id),
	CONSTRAINT FK__PlayHisto__Media__1332DBDC FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id),
	CONSTRAINT FK__PlayHisto__UserI__123EB7A3 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)
);
 CREATE NONCLUSTERED INDEX IX_PlayHistory_UserId ON TuneVault.dbo.PlayHistory (  UserId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;


-- TuneVault.dbo.Playlists definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Playlists;

CREATE TABLE TuneVault.dbo.Playlists (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Title nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Description nvarchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CoverImageUrl varchar(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsPublic bit DEFAULT 1 NOT NULL,
	CreatedAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	ReleaseDate datetime2 NULL,
	ContentType tinyint NULL,
	CONSTRAINT PK__Playlist__3214EC0783895B2B PRIMARY KEY (Id),
	CONSTRAINT FK__Playlists__UserI__74AE54BC FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.UserAccountTiers definition

-- Drop table

-- DROP TABLE TuneVault.dbo.UserAccountTiers;

CREATE TABLE TuneVault.dbo.UserAccountTiers (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TierId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PriceAmount decimal(10,2) NOT NULL,
	PriceCurrency varchar(5) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT 'USD' NOT NULL,
	PurchasedAt datetime2 NOT NULL,
	ActivatedAt datetime2 NOT NULL,
	ExpiresAt datetime2 NOT NULL,
	IsActive bit DEFAULT 1 NOT NULL,
	CONSTRAINT PK__UserAcco__3214EC073ABA82B7 PRIMARY KEY (Id),
	CONSTRAINT FK__UserAccou__TierI__5BE2A6F2 FOREIGN KEY (TierId) REFERENCES TuneVault.dbo.AccountTiers(Id),
	CONSTRAINT FK__UserAccou__UserI__5AEE82B9 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)
);


-- TuneVault.dbo.AlbumTracks definition

-- Drop table

-- DROP TABLE TuneVault.dbo.AlbumTracks;

CREATE TABLE TuneVault.dbo.AlbumTracks (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	AlbumId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	MediaItemId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TrackOrder int NOT NULL,
	AddedAt datetime2 NOT NULL,
	CONSTRAINT PK__AlbumTra__3214EC0789C8D90A PRIMARY KEY (Id),
	CONSTRAINT FK__AlbumTrac__Album__70DDC3D8 FOREIGN KEY (AlbumId) REFERENCES TuneVault.dbo.Albums(Id),
	CONSTRAINT FK__AlbumTrac__Media__71D1E811 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)
);


-- TuneVault.dbo.Favorites definition

-- Drop table

-- DROP TABLE TuneVault.dbo.Favorites;

CREATE TABLE TuneVault.dbo.Favorites (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Reaction tinyint DEFAULT 1 NOT NULL,
	LikedAt datetime2 NOT NULL,
	TargetId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TargetType tinyint DEFAULT 0 NOT NULL,
	CONSTRAINT PK__Favorite__3214EC0704049E32 PRIMARY KEY (Id),
	CONSTRAINT FK__Favorites__UserI__7C4F7684 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)
);
 CREATE NONCLUSTERED INDEX IX_Favorites_UserId ON TuneVault.dbo.Favorites (  UserId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
 CREATE NONCLUSTERED INDEX IX_Favorites_User_Target ON TuneVault.dbo.Favorites (  UserId ASC  , TargetType ASC  , TargetId ASC  )  
	 WITH (  PAD_INDEX = OFF ,FILLFACTOR = 100  ,SORT_IN_TEMPDB = OFF , IGNORE_DUP_KEY = OFF , STATISTICS_NORECOMPUTE = OFF , ONLINE = OFF , ALLOW_ROW_LOCKS = ON , ALLOW_PAGE_LOCKS = ON  )
	 ON [PRIMARY ] ;
ALTER TABLE TuneVault.dbo.Favorites WITH NOCHECK ADD CONSTRAINT CK_Favorites_TargetType CHECK (([TargetType]=(2) OR [TargetType]=(1) OR [TargetType]=(0)));


-- TuneVault.dbo.MediaArtists definition

-- Drop table

-- DROP TABLE TuneVault.dbo.MediaArtists;

CREATE TABLE TuneVault.dbo.MediaArtists (
	MediaItemId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ArtistId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Role] nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS DEFAULT 'MainArtist' NOT NULL,
	CONSTRAINT PK_MediaArtists PRIMARY KEY (MediaItemId,ArtistId),
	CONSTRAINT FK__MediaArti__Artis__6A30C649 FOREIGN KEY (ArtistId) REFERENCES TuneVault.dbo.Users(Id),
	CONSTRAINT FK__MediaArti__Media__693CA210 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)
);


-- TuneVault.dbo.PlaylistTracks definition

-- Drop table

-- DROP TABLE TuneVault.dbo.PlaylistTracks;

CREATE TABLE TuneVault.dbo.PlaylistTracks (
	Id varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PlaylistId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	MediaItemId varchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TrackOrder int NOT NULL,
	AddedAt datetime2 NOT NULL,
	CONSTRAINT PK__Playlist__3214EC075859AE87 PRIMARY KEY (Id),
	CONSTRAINT FK__PlaylistT__Media__797309D9 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id),
	CONSTRAINT FK__PlaylistT__Playl__787EE5A0 FOREIGN KEY (PlaylistId) REFERENCES TuneVault.dbo.Playlists(Id)
);


ALTER TABLE TuneVault.dbo.MediaItems
ADD CONSTRAINT CK_MediaItems_MediaType CHECK (MediaType IN (0, 1, 2, 3));

ALTER TABLE TuneVault.dbo.MediaItems
ADD CONSTRAINT CK_MediaItems_AccessLevel CHECK (AccessLevel IN (0, 1));

ALTER TABLE TuneVault.dbo.Albums
ADD CONSTRAINT CK_Albums_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 2, 3));

ALTER TABLE TuneVault.dbo.Playlists
ADD CONSTRAINT CK_Playlists_ContentType CHECK (ContentType IS NULL OR ContentType IN (0, 1, 2, 3));

ALTER TABLE TuneVault.dbo.Friends
ADD CONSTRAINT CK_Friends_Status CHECK (Status IN (1, 2, 3));

ALTER TABLE TuneVault.dbo.MediaShares
ADD CONSTRAINT CK_MediaShares_ShareType CHECK (ShareType IN (1, 2, 3));

ALTER TABLE TuneVault.dbo.Notifications
ADD CONSTRAINT CK_Notifications_NotifyType CHECK (NotifyType IN (1, 2, 3, 4, 5, 6));

ALTER TABLE TuneVault.dbo.Notifications
ADD CONSTRAINT CK_Notifications_TargetType CHECK (TargetType IS NULL OR TargetType IN (1, 2, 3));

ALTER TABLE TuneVault.dbo.Ads
ADD CONSTRAINT CK_Ads_AdType CHECK (AdType IN (1, 2, 3));

ALTER TABLE TuneVault.dbo.OtpLogs
ADD CONSTRAINT CK_OtpLogs_Purpose CHECK (Purpose IN ('register', 'reset_password', 'change_password'));

ALTER TABLE TuneVault.dbo.Favorites
ADD CONSTRAINT CK_Favorites_Reaction CHECK (Reaction IN (0, 1, 2, 3, 4, 5));
