# TuneVault Endpoint Flows

Tài liệu này liệt kê luồng hoạt động của các endpoint chính trong TuneVault theo thứ tự:

`Frontend component -> frontend/Services/MediaService.tsx -> API controller -> Application command/query/handler -> Infrastructure repository/service -> Database / file storage`

Ghi chú:

- `frontend/src/React/App.tsx` là nơi điều phối trạng thái chung của UI.
- Các component con là nơi phát sinh thao tác người dùng.
- Một số route streaming/poster không đi qua repository để ghi dữ liệu, mà đọc metadata từ DB rồi trả file từ `wwwroot/uploads`.
- Các route như `/` và `/health` không đi qua database.

## 1. Utility endpoints

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /` | Không có UI trực tiếp | `backend/src/TuneVault.API/Program.cs` | Không có | Không có DB |
| `GET /health` | Không có UI trực tiếp | `backend/src/TuneVault.API/Program.cs` | Không có | Không có DB |

## 2. Auth

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `POST /api/auth/logout` | `frontend/src/React/App.tsx` -> `logoutUser()` | `backend/src/TuneVault.API/Controllers/AuthController.cs::Logout()` | Không qua MediatR | Chỉ xóa cookie `tunevault_access_token` và `tunevault_refresh_token` |
| `POST /api/auth/login` | `frontend/src/React/Components/AuthLoginModal.jsx` -> `loginUser()` | `AuthController.cs::Login()` | `backend/src/TuneVault.Application/Features/Auth/Commands/Login/LoginCommand.cs`, `LoginCommandHandler.cs`, `LoginCommandValidator.cs` | `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`, `backend/src/TuneVault.Infrastructure/Authentication/JwtTokenGenerator.cs`; đọc `dbo.Users` |
| `POST /api/auth/send-otp` | `AuthLoginModal.jsx` -> `sendOtp()` | `AuthController.cs::SendOtp()` | `backend/src/TuneVault.Application/Features/Auth/Commands/SendOtp/SendOtpCommand.cs`, `SendOtpCommandHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/OtpLogRepository.cs`, `backend/src/TuneVault.Infrastructure/Services/GmailSmtpEmailService.cs`; ghi `dbo.OtpLogs` |
| `POST /api/auth/register` | `AuthLoginModal.jsx` -> `registerUser()` | `AuthController.cs::Register()` | `backend/src/TuneVault.Application/Features/Auth/Commands/Register/RegisterCommand.cs`, `RegisterCommandHandler.cs` | `UserRepository.cs`, `OtpLogRepository.cs`, `JwtTokenGenerator.cs`; ghi `dbo.Users`, đọc/xóa OTP trong `dbo.OtpLogs` |
| `POST /api/auth/reset-password` | `AuthLoginModal.jsx` -> `sendOtp()` rồi `reset password` flow trong cùng modal | `AuthController.cs::ResetPassword()` | `backend/src/TuneVault.Application/Features/Auth/Commands/ResetPassword/ResetPasswordCommand.cs`, `ResetPasswordCommandHandler.cs` | `UserRepository.cs`, `OtpLogRepository.cs`; đọc/ghi `dbo.Users`, đọc/xóa `dbo.OtpLogs` |
| `POST /api/auth/change-password` | `AuthLoginModal.jsx` -> `changePassword()` | `AuthController.cs::ChangePassword()` | `backend/src/TuneVault.Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommand.cs`, `ChangePasswordCommandHandler.cs` | `UserRepository.cs`, `OtpLogRepository.cs`; đọc/ghi `dbo.Users`, đọc/xóa `dbo.OtpLogs` |
| `POST /api/auth/refresh` | `frontend/src/React/App.tsx` -> `refreshAuthSession()` | `AuthController.cs::Refresh()` | Không qua MediatR | Chỉ validate JWT refresh token bằng `JwtTokenGenerator`; không ghi DB |

## 3. Users, profile, follow

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/users/me/profile` | `frontend/src/React/Components/ProfileView.jsx` -> `getMyProfile()` | `backend/src/TuneVault.API/Controllers/UsersController.cs::GetMyProfile()` | `backend/src/TuneVault.Application/Features/User/Queries/GetProfile/GetUserProfileQuery.cs`, `GetUserProfileQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`; đọc `dbo.Users` |
| `PUT /api/users/me/profile` | `ProfileView.jsx` -> `updateProfile()` | `UsersController.cs::UpdateMyProfile()` | `backend/src/TuneVault.Application/Features/User/Commands/UpdateProfile/UpdateProfileCommand.cs`, `UpdateProfileCommandHandler.cs` | `UserRepository.cs`, `backend/src/TuneVault.Infrastructure/Services/LocalFileStorageService.cs`; ghi `dbo.Users`, avatar lưu tại `backend/src/TuneVault.API/wwwroot/uploads/avatars` |
| `GET /api/users/artists` | `frontend/src/React/Components/Home.jsx`, `ProfileView.jsx` -> `getArtists()` | `UsersController.cs::GetArtists()` | `backend/src/TuneVault.Application/Features/User/Queries/GetAllArtists/GetAllArtistsQuery.cs`, `GetAllArtistsQueryHandler.cs` | `UserRepository.cs`; đọc `dbo.Users` |
| `GET /api/users/by-handle/{idDisplay}` | `frontend/src/React/Components/AuthLoginModal.jsx` -> `getUserByIdDisplay()` | `UsersController.cs::GetUserByIdDisplay()` | `backend/src/TuneVault.Application/Features/User/Queries/GetUserByIdDisplay/GetUserByIdDisplayQuery.cs`, `GetUserByIdDisplayQueryHandler.cs` | `UserRepository.cs`; đọc `dbo.Users` |
| `GET /api/users/{id}` | `ProfileView.jsx`, `Home.jsx` -> `getUserById()` | `UsersController.cs::GetUserById()` | `backend/src/TuneVault.Application/Features/User/Queries/GetUserById/GetUserByIdQuery.cs`, `GetUserByIdQueryHandler.cs` | `UserRepository.cs`; đọc `dbo.Users` |
| `POST /api/users/follow` | `ProfileView.jsx` -> `followUser()` | `UsersController.cs::FollowUser()` | `backend/src/TuneVault.Application/Features/User/Commands/FollowUser/FollowUserCommand.cs`, `FollowUserCommandHandler.cs` | `UserRepository.cs`, `FollowRepository.cs`; ghi `dbo.Users` và `dbo.Follows` |
| `DELETE /api/users/unfollow` | `ProfileView.jsx` -> `unfollowUser()` | `UsersController.cs::UnfollowUser()` | `backend/src/TuneVault.Application/Features/User/Commands/UnfollowUser/UnfollowUserCommand.cs`, `UnfollowUserCommandHandler.cs` | `UserRepository.cs`, `FollowRepository.cs`; ghi `dbo.Users`, xóa trong `dbo.Follows` |
| `GET /api/users/is-following/{followeeId}` | `ProfileView.jsx` -> `checkFollowStatus()` | `UsersController.cs::GetFollowStatus()` | `backend/src/TuneVault.Application/Features/User/Queries/CheckFollowStatus/CheckFollowStatusQuery.cs`, `CheckFollowStatusQueryHandler.cs` | `UserRepository.cs`, `FollowRepository.cs`; đọc `dbo.Follows` |

## 4. Media

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/media/{id}` | `frontend/src/React/Components/Home.jsx`, `ManageStudio.jsx`, `LibraryDetailView.jsx`, `PlaylistModal.jsx`, `ShareActivity.jsx` -> `getTrackById()` / `getMediaById()` | `backend/src/TuneVault.API/Controllers/MediaController.cs::GetById()` | `backend/src/TuneVault.Application/Features/Media/Queries/GetMediaById/GetMediaByIdQuery.cs`, `GetMediaByIdQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/MediaRepository.cs`; đọc `dbo.MediaItems` |
| `GET /api/media/{id}/audio/stream` | `frontend/src/React/App.tsx` -> `mediaAudioStreamUrl()` | `MediaController.cs::StreamAudio()` | `backend/src/TuneVault.Application/Features/Media/Queries/GetMediaStream/GetMediaStreamQuery.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; metadata từ `dbo.MediaItems`, file từ `backend/src/TuneVault.API/wwwroot/uploads` |
| `GET /api/media/{id}/video/stream` | `App.tsx` -> `mediaVideoStreamUrl()` | `MediaController.cs::StreamVideo()` | `GetMediaStreamQuery.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; metadata từ `dbo.MediaItems`, file từ `wwwroot/uploads` |
| `GET /api/media/{id}/poster` | `App.tsx` / `MediaInfoPanel.jsx` -> `mediaPosterUrl()` | `MediaController.cs::Poster()` | `GetMediaStreamQuery.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; metadata từ `dbo.MediaItems`, poster từ `wwwroot/uploads` |
| `GET /api/media` | `frontend/src/React/Components/Home.jsx`, `frontend/src/React/App.tsx` -> `getMedia()` | `MediaController.cs::GetMedia()` | `backend/src/TuneVault.Application/Features/Media/Queries/GetMedia/GetMediaQuery.cs`, `GetMediaQueryHandler.cs` | `MediaRepository.cs`; đọc `dbo.MediaItems` |
| `GET /api/media/my-media` | `Sidebar.jsx`, `ManageStudio.jsx`, `ProfileView.jsx` -> `getMyMedia()` | `MediaController.cs::GetUserMedia()` | `backend/src/TuneVault.Application/Features/Media/Queries/GetUserMedia/GetUserMediaQuery.cs`, `GetUserMediaQueryHandler.cs` | `MediaRepository.cs`; đọc `dbo.MediaItems` |
| `GET /api/media/artist/{userId}` | `ProfileView.jsx` -> `getArtistMedia()` | `MediaController.cs::GetUserMediaByArtist()` | `backend/src/TuneVault.Application/Features/Media/Queries/GetArtistMedia/GetArtistMediaQuery.cs`, `GetArtistMediaQueryHandler.cs` | `MediaRepository.cs`; đọc `dbo.MediaItems` |
| `POST /api/media/upload` | `ManageStudio.jsx` -> `uploadMedia()` | `MediaController.cs::Upload()` | `backend/src/TuneVault.Application/Features/Media/Commands/GenerateMediaId/GenerateMediaIdCommand.cs`, `GenerateMediaIdCommandHandler.cs`, `UploadMediaCommand.cs`, `UploadMediaCommandHandler.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; ghi `dbo.MediaItems`, file vào `wwwroot/uploads` |
| `POST /api/media/upload/audio` | `ManageStudio.jsx` -> `uploadMedia('audio')` | `MediaController.cs::UploadAudio()` | `GenerateMediaIdCommand`, `UploadMediaCommand` | `MediaRepository.cs`, `LocalFileStorageService.cs`; ghi `dbo.MediaItems`, file vào `wwwroot/uploads` |
| `POST /api/media/upload/video` | `ManageStudio.jsx` -> `uploadMedia('video')` | `MediaController.cs::UploadVideo()` | `GenerateMediaIdCommand`, `UploadMediaCommand` | `MediaRepository.cs`, `LocalFileStorageService.cs`; ghi `dbo.MediaItems`, file vào `wwwroot/uploads` |
| `PUT /api/media/{id}` | `ManageStudio.jsx` -> `updateMedia()` | `MediaController.cs::Update()` | `backend/src/TuneVault.Application/Features/Media/Commands/UpdateMedia/UpdateMediaCommand.cs`, `UpdateMediaCommandHandler.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; cập nhật `dbo.MediaItems`, có thể thay file trong `wwwroot/uploads` |
| `DELETE /api/media/{id}` | `ManageStudio.jsx` -> `deleteMedia()` | `MediaController.cs::Delete()` | `backend/src/TuneVault.Application/Features/Media/Commands/DeleteMedia/DeleteMediaCommand.cs`, `DeleteMediaCommandHandler.cs` | `MediaRepository.cs`, `LocalFileStorageService.cs`; cập nhật/xóa logic soft delete trong `dbo.MediaItems`, dọn file upload |

## 5. Playlists

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/playlists` | `frontend/src/React/Components/Home.jsx`, `Sidebar.jsx`, `LibraryDetailView.jsx`, `ProfileView.jsx` -> `getFeaturedPlaylists()` | `backend/src/TuneVault.API/Controllers/PlaylistController.cs::GetPublic()` | `backend/src/TuneVault.Application/Features/Playlist/Queries/GetPublicPlaylists/GetPublicPlaylistsQuery.cs`, `GetPublicPlaylistsQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/PlaylistRepository.cs`; đọc `dbo.Playlists`, `dbo.PlaylistTracks`, `dbo.MediaItems` |
| `GET /api/playlists/me` | `Sidebar.jsx`, `ManageStudio.jsx`, `ProfileView.jsx` -> `getMyPlaylists()` | `PlaylistController.cs::GetMine()` | `backend/src/TuneVault.Application/Features/Playlist/Queries/GetPlaylists/GetPlaylistsQuery.cs`, `GetPlaylistsQueryHandler.cs` | `PlaylistRepository.cs`; đọc `dbo.Playlists` |
| `GET /api/playlists/{id}` | `LibraryDetailView.jsx`, `PlaylistModal.jsx` -> `getPlaylistById()` | `PlaylistController.cs::GetById()` | `backend/src/TuneVault.Application/Features/Playlist/Queries/GetPlaylistById/GetPlaylistByIdQuery.cs`, `GetPlaylistByIdQueryHandler.cs` | `PlaylistRepository.cs`; đọc `dbo.Playlists`, `dbo.PlaylistTracks`, `dbo.MediaItems` |
| `POST /api/playlists` | `ManageStudio.jsx` -> `createPlaylist()` | `PlaylistController.cs::Create()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/CreatePlaylist/CreatePlaylistCommand.cs`, `CreatePlaylistCommandHandler.cs` | `PlaylistRepository.cs`, `LocalFileStorageService.cs`; ghi `dbo.Playlists` |
| `PUT /api/playlists/{id}` | `ManageStudio.jsx` -> `updatePlaylist()` | `PlaylistController.cs::Update()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/UpdatePlaylist/UpdatePlaylistCommand.cs`, `UpdatePlaylistCommandHandler.cs` | `PlaylistRepository.cs`, `LocalFileStorageService.cs`; cập nhật `dbo.Playlists` |
| `DELETE /api/playlists/{id}` | `ManageStudio.jsx` -> `deletePlaylist()` | `PlaylistController.cs::Delete()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/DeletePlaylist/DeletePlaylistCommand.cs`, `DeletePlaylistCommandHandler.cs` | `PlaylistRepository.cs`; xóa `dbo.PlaylistTracks` và `dbo.Playlists` |
| `POST /api/playlists/{id}/tracks` | `frontend/src/React/App.tsx` và `ManageStudio.jsx` -> `addTrackToPlaylist()` | `PlaylistController.cs::AddTrack()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/AddTrackToPlaylist/AddTrackToPlaylistCommand.cs`, `AddTrackToPlaylistCommandHandler.cs` | `PlaylistRepository.cs`; ghi `dbo.PlaylistTracks` |
| `DELETE /api/playlists/{id}/tracks/{mediaId}` | `ManageStudio.jsx` -> `removeTrackFromPlaylist()` | `PlaylistController.cs::RemoveTrack()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/RemoveTrackFromPlaylist/RemoveTrackFromPlaylistCommand.cs`, `RemoveTrackFromPlaylistCommandHandler.cs` | `PlaylistRepository.cs`; xóa `dbo.PlaylistTracks` |
| `PATCH /api/playlists/{playlistId}/tracks/{mediaItemId}/order` | `LibraryDetailView.jsx`, `ManageStudio.jsx` -> `reorderTrackInPlaylist()` | `PlaylistController.cs::UpdateTrackOrder()` | `backend/src/TuneVault.Application/Features/Playlist/Commands/UpdateTrackOrder/UpdateTrackOrderCommand.cs`, `UpdateTrackOrderCommandHandler.cs` | `PlaylistRepository.cs`; cập nhật `dbo.PlaylistTracks.TrackOrder` |

## 6. Albums

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/albums` | `frontend/src/React/Components/Home.jsx`, `Sidebar.jsx`, `LibraryDetailView.jsx`, `ProfileView.jsx` -> `getFeaturedAlbums()` | `backend/src/TuneVault.API/Controllers/AlbumsController.cs::GetPublic()` | `backend/src/TuneVault.Application/Features/Album/Queries/GetPublicAlbums/GetPublicAlbumsQuery.cs`, `GetPublicAlbumsQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/AlbumRepository.cs`; đọc `dbo.Albums`, `dbo.AlbumTracks`, `dbo.MediaItems` |
| `GET /api/albums/me` | `Sidebar.jsx`, `ManageStudio.jsx`, `ProfileView.jsx` -> `getMyAlbums()` | `AlbumsController.cs::GetMine()` | `backend/src/TuneVault.Application/Features/Album/Queries/GetMyAlbums/GetMyAlbumsQuery.cs`, `GetMyAlbumsQueryHandler.cs` | `AlbumRepository.cs`; đọc `dbo.Albums` |
| `GET /api/albums/{id}` | `LibraryDetailView.jsx`, `ProfileView.jsx` -> `getAlbumById()` | `AlbumsController.cs::GetById()` | `backend/src/TuneVault.Application/Features/Album/Queries/GetAlbumById/GetAlbumByIdQuery.cs`, `GetAlbumByIdQueryHandler.cs` | `AlbumRepository.cs`; đọc `dbo.Albums`, `dbo.AlbumTracks`, `dbo.MediaItems` |
| `POST /api/albums` | `ManageStudio.jsx` -> `createAlbum()` | `AlbumsController.cs::Create()` | `backend/src/TuneVault.Application/Features/Album/Commands/CreateAlbum/CreateAlbumCommand.cs`, `CreateAlbumCommandHandler.cs` | `AlbumRepository.cs`, `LocalFileStorageService.cs`; ghi `dbo.Albums` |
| `PUT /api/albums/{id}` | `ManageStudio.jsx` -> `updateAlbum()` | `AlbumsController.cs::Update()` | `backend/src/TuneVault.Application/Features/Album/Commands/UpdateAlbum/UpdateAlbumCommand.cs`, `UpdateAlbumCommandHandler.cs` | `AlbumRepository.cs`, `LocalFileStorageService.cs`; cập nhật `dbo.Albums` |
| `DELETE /api/albums/{id}` | `ManageStudio.jsx` -> `deleteAlbum()` | `AlbumsController.cs::Delete()` | `backend/src/TuneVault.Application/Features/Album/Commands/DeleteAlbum/DeleteAlbumCommand.cs`, `DeleteAlbumCommandHandler.cs` | `AlbumRepository.cs`; xóa `dbo.AlbumTracks` và `dbo.Albums` |
| `POST /api/albums/{id}/tracks` | `ManageStudio.jsx` -> `addTrackToAlbum()` | `AlbumsController.cs::AddTrack()` | `backend/src/TuneVault.Application/Features/Album/Commands/AddTrackToAlbum/AddTrackToAlbumCommand.cs`, `AddTrackToAlbumCommandHandler.cs` | `AlbumRepository.cs`; ghi `dbo.AlbumTracks` |
| `DELETE /api/albums/{id}/tracks/{mediaId}` | `ManageStudio.jsx` -> `removeTrackFromAlbum()` | `AlbumsController.cs::RemoveTrack()` | `backend/src/TuneVault.Application/Features/Album/Commands/RemoveTrackFromAlbum/RemoveTrackFromAlbumCommand.cs`, `RemoveTrackFromAlbumCommandHandler.cs` | `AlbumRepository.cs`; xóa `dbo.AlbumTracks` |
| `PATCH /api/albums/{albumId}/tracks/{mediaId}/order` | `ManageStudio.jsx` -> `reorderTrackInAlbum()` | `AlbumsController.cs::UpdateTrackOrder()` | `backend/src/TuneVault.Application/Features/Album/Commands/UpdateAlbumTrackOrder/UpdateAlbumTrackOrderCommand.cs`, `UpdateAlbumTrackOrderCommandHandler.cs` | `AlbumRepository.cs`; cập nhật `dbo.AlbumTracks.TrackOrder` |

## 7. Search, favorites, collection likes

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/search` | `frontend/src/React/Components/Home.jsx` -> `searchAll()`; `ProfileView.jsx` và `FriendActivity.jsx` dùng `searchUsers()` nhưng helper này gọi lại `searchAll()` | `backend/src/TuneVault.API/Controllers/SearchController.cs::Search()` | `backend/src/TuneVault.Application/Features/Search/Queries/SearchMedia/SearchMediaQuery.cs`, `SearchMediaQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/SearchRepository.cs`; đọc `dbo.Users`, `dbo.MediaItems`, `dbo.Playlists`, `dbo.Albums` |
| `GET /api/search/trending` | `Home.jsx` -> `getTrendingTracks()` | `SearchController.cs::GetTrending()` | `backend/src/TuneVault.Application/Features/Search/Queries/GetTrendingMedia/GetTrendingMediaQuery.cs`, `GetTrendingMediaQueryHandler.cs` | `SearchRepository.cs`; đọc `dbo.MediaItems` |
| `PUT /api/favorites/{mediaId}` | `frontend/src/React/App.tsx` và `Home.jsx` -> `toggleFavorite()` | `backend/src/TuneVault.API/Controllers/FavoriteController.cs::React()` | `backend/src/TuneVault.Application/Features/Favorite/Commands/ToggleFavorite/ToggleFavoriteCommand.cs`, `ToggleFavoriteCommandHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/FavoriteRepository.cs`; ghi `dbo.Favorites`, có thể cập nhật `dbo.MediaItems` |
| `DELETE /api/favorites/{mediaId}` | `App.tsx` / `Home.jsx` -> `toggleFavorite(false)` | `FavoriteController.cs::Unlike()` | `ToggleFavoriteCommand.cs`, `ToggleFavoriteCommandHandler.cs` | `FavoriteRepository.cs`; cập nhật `dbo.Favorites` |
| `GET /api/favorites/me` | `frontend/src/React/Components/Sidebar.jsx` -> `getLikedSongs()` | `FavoriteController.cs::GetFavorites()` | `backend/src/TuneVault.Application/Features/Favorite/Queries/GetFavorites/GetFavoritesQuery.cs`, `GetFavoritesQueryHandler.cs` | `FavoriteRepository.cs`, `MediaRepository.cs`; đọc `dbo.Favorites`, `dbo.MediaItems` |
| `GET /api/favorites/status/{mediaId}` | `frontend/src/React/App.tsx` -> `getFavoriteStatus()` | `FavoriteController.cs::CheckFavoriteStatus()` | `backend/src/TuneVault.Application/Features/Favorite/Queries/CheckFavoriteStatus/CheckFavoriteStatusQuery.cs`, `CheckFavoriteStatusQueryHandler.cs` | `FavoriteRepository.cs`; đọc `dbo.Favorites` |
| `GET /api/favorites/{mediaId}/reaction-count` | `Home.jsx`, `App.tsx`, `MediaInfoPanel.jsx`, `ReactionSummary.jsx` -> `getMediaReactionCount()` | `FavoriteController.cs::CountMediaReactions()` | `backend/src/TuneVault.Application/Features/Favorite/Queries/CountFavoriteReactions/CountFavoriteReactionsQuery.cs`, `CountFavoriteReactionsQueryHandler.cs` | `FavoriteRepository.cs`; đếm `dbo.Favorites` |
| `GET /api/favorites/albums/{albumId}/reaction-count` | `Home.jsx` -> `getAlbumReactionCount()` | `FavoriteController.cs::CountAlbumReactions()` | `CountFavoriteReactionsQuery.cs`, `CountFavoriteReactionsQueryHandler.cs` | `FavoriteRepository.cs`; đếm `dbo.Favorites` theo `TargetType = Album` |
| `GET /api/favorites/playlists/{playlistId}/reaction-count` | `Home.jsx` -> `getPlaylistReactionCount()` | `FavoriteController.cs::CountPlaylistReactions()` | `CountFavoriteReactionsQuery.cs`, `CountFavoriteReactionsQueryHandler.cs` | `FavoriteRepository.cs`; đếm `dbo.Favorites` theo `TargetType = Playlist` |
| `GET /api/collection-likes/recent` | `frontend/src/React/Components/Sidebar.jsx` -> `getRecentCollectionLikes()` | `backend/src/TuneVault.API/Controllers/CollectionLikesController.cs::GetRecent()` | `backend/src/TuneVault.Application/Features/CollectionLike/Queries/GetRecentCollectionLikes/GetRecentCollectionLikesQuery.cs`, `GetRecentCollectionLikesQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/CollectionLikeRepository.cs`; đọc `dbo.CollectionLikes` |
| `PUT /api/collection-likes` | `frontend/src/Services/MediaService.tsx` -> `toggleCollectionLike()` (chưa thấy component gọi trực tiếp) | `CollectionLikesController.cs::Toggle()` | `backend/src/TuneVault.Application/Features/CollectionLike/Commands/ToggleCollectionLike/ToggleCollectionLikeCommand.cs`, `ToggleCollectionLikeCommandHandler.cs` | `CollectionLikeRepository.cs`; ghi `dbo.CollectionLikes` |

## 8. Notifications

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `GET /api/notifications` | `frontend/src/React/Components/NotificationActivity.jsx` -> `getNotifications()` | `backend/src/TuneVault.API/Controllers/NotificationController.cs::GetAll()` | `backend/src/TuneVault.Application/Features/Notification/Queries/GetNotifications/GetNotificationsQuery.cs`, `GetNotificationsQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/NotificationRepository.cs`; đọc `dbo.Notifications`, `dbo.Users`, `dbo.MediaItems` |
| `GET /api/notifications/unread` | `NotificationActivity.jsx` -> `getUnreadNotifications()` | `NotificationController.cs::GetUnread()` | `backend/src/TuneVault.Application/Features/Notification/Queries/GetUnreadNotifications/GetUnreadNotificationsQuery.cs`, `GetUnreadNotificationsQueryHandler.cs` | `NotificationRepository.cs`; đọc `dbo.Notifications` |
| `PATCH /api/notifications/{notificationId}/read` | `NotificationActivity.jsx` -> `markNotificationAsRead()` | `NotificationController.cs::MarkAsRead()` | `backend/src/TuneVault.Application/Features/Notification/Commands/MarkAsRead/MarkNotificationAsReadCommand.cs`, `MarkNotificationAsReadCommandHandler.cs` | `NotificationRepository.cs`; cập nhật `dbo.Notifications.IsRead` |
| `PATCH /api/notifications/read-all` | `NotificationActivity.jsx` -> `markAllNotificationsAsRead()` | `NotificationController.cs::MarkAllAsRead()` | `backend/src/TuneVault.Application/Features/Notification/Commands/MarkAsRead/MarkAllNotificationsAsReadCommand.cs`, `MarkAllNotificationsAsReadCommandHandler.cs` | `NotificationRepository.cs`; cập nhật `dbo.Notifications.IsRead` |
| `DELETE /api/notifications/{notificationId}` | `NotificationActivity.jsx` -> `deleteNotification()` | `NotificationController.cs::Delete()` | `backend/src/TuneVault.Application/Features/Notification/Commands/DeleteNotification/DeleteNotificationCommand.cs`, `DeleteNotificationCommandHandler.cs` | `NotificationRepository.cs`; xóa trong `dbo.Notifications` |

Realtime liên quan:

- `backend/src/TuneVault.Infrastructure/Realtime/NotificationHub.cs`
- `backend/src/TuneVault.Infrastructure/Realtime/SignalRNotificationPusher.cs`

## 9. Shares, friends, history

| Endpoint | Frontend | Backend | Application | Infrastructure / DB |
|---|---|---|---|---|
| `POST /api/shares` | `frontend/src/React/App.tsx` -> `shareItem()`; `FriendActivity.jsx` nhận metadata item đang chia sẻ | `backend/src/TuneVault.API/Controllers/ShareController.cs::Share()` | `backend/src/TuneVault.Application/Features/Share/Commands/ShareMedia/ShareMediaCommand.cs`, `ShareMediaCommandHandler.cs`, `ShareMediaCommandValidator.cs` | `backend/src/TuneVault.Infrastructure/Repositories/MediaShareRepository.cs`; ghi `dbo.MediaShares` và tạo `dbo.Notifications` |
| `GET /api/shares/inbox` | `frontend/src/React/Components/ShareActivity.jsx` -> `getShareInbox()` | `ShareController.cs::GetInbox()` | `backend/src/TuneVault.Application/Features/Share/Queries/GetSharedWithMe/GetSharedWithMeQuery.cs`, `GetSharedWithMeQueryHandler.cs` | `MediaShareRepository.cs`; đọc `dbo.MediaShares`, `dbo.Users`, `dbo.MediaItems` |
| `GET /api/shares/sent` | `ShareActivity.jsx` -> `getShareSent()` | `ShareController.cs::GetSent()` | `backend/src/TuneVault.Application/Features/Share/Queries/GetSharedByMe/GetSharedByMeQuery.cs`, `GetSharedByMeQueryHandler.cs` | `MediaShareRepository.cs`; đọc `dbo.MediaShares` |
| `GET /api/friends/me` | `frontend/src/React/Components/FriendActivity.jsx` -> `getMyFriends()` | `backend/src/TuneVault.API/Controllers/FriendsController.cs::GetMyFriends()` | `backend/src/TuneVault.Application/Features/Friend/Queries/GetMyFriends/GetMyFriendsQuery.cs`, `GetMyFriendsQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/FriendRepository.cs`; đọc `dbo.Friends`, `dbo.Users` |
| `GET /api/friends/requests/inbox` | `FriendActivity.jsx` -> `getIncomingFriendRequests()` | `FriendsController.cs::GetIncomingRequests()` | `backend/src/TuneVault.Application/Features/Friend/Queries/GetIncomingFriendRequests/GetIncomingFriendRequestsQuery.cs`, `GetIncomingFriendRequestsQueryHandler.cs` | `FriendRepository.cs`; đọc `dbo.Friends`, `dbo.Users` |
| `GET /api/friends/requests/sent` | `FriendActivity.jsx` -> `getSentFriendRequests()` | `FriendsController.cs::GetSentRequests()` | `backend/src/TuneVault.Application/Features/Friend/Queries/GetSentFriendRequests/GetSentFriendRequestsQuery.cs`, `GetSentFriendRequestsQueryHandler.cs` | `FriendRepository.cs`; đọc `dbo.Friends`, `dbo.Users` |
| `POST /api/friends/requests/{receiverId}` | `FriendActivity.jsx` -> `sendFriendRequest()` | `FriendsController.cs::SendRequest()` | `backend/src/TuneVault.Application/Features/Friend/Commands/SendFriendRequest/SendFriendRequestCommand.cs`, `SendFriendRequestCommandHandler.cs` | `FriendRepository.cs`; ghi `dbo.Friends`, có thể tạo `dbo.Notifications` |
| `POST /api/friends/requests/{requestId}/accept` | `FriendActivity.jsx` -> `acceptFriendRequest()` | `FriendsController.cs::AcceptRequest()` | `backend/src/TuneVault.Application/Features/Friend/Commands/AcceptFriendRequest/AcceptFriendRequestCommand.cs`, `AcceptFriendRequestCommandHandler.cs` | `FriendRepository.cs`; cập nhật `dbo.Friends`, có thể cập nhật `dbo.Users` |
| `POST /api/friends/requests/{requestId}/reject` | `FriendActivity.jsx` -> `rejectFriendRequest()` | `FriendsController.cs::RejectRequest()` | `backend/src/TuneVault.Application/Features/Friend/Commands/RejectFriendRequest/RejectFriendRequestCommand.cs`, `RejectFriendRequestCommandHandler.cs` | `FriendRepository.cs`; cập nhật `dbo.Friends` |
| `DELETE /api/friends/requests/{requestId}` | `FriendActivity.jsx` -> `cancelFriendRequest()` | `FriendsController.cs::CancelRequest()` | `backend/src/TuneVault.Application/Features/Friend/Commands/CancelFriendRequest/CancelFriendRequestCommand.cs`, `CancelFriendRequestCommandHandler.cs` | `FriendRepository.cs`; xóa/cập nhật `dbo.Friends` |
| `DELETE /api/friends/{friendUserId}` | `FriendActivity.jsx` -> `removeFriend()` | `FriendsController.cs::RemoveFriend()` | `backend/src/TuneVault.Application/Features/Friend/Commands/RemoveFriend/RemoveFriendCommand.cs`, `RemoveFriendCommandHandler.cs` | `FriendRepository.cs`; cập nhật `dbo.Friends` |
| `GET /api/history/recent` | `frontend/src/React/Components/ListeningHistoryActivity.jsx` -> `getRecentHistory()`; `frontend/src/React/App.tsx` cũng dùng để dựng queue | `backend/src/TuneVault.API/Controllers/HistoryController.cs::GetRecentHistory()` | `backend/src/TuneVault.Application/Features/History/Queries/GetRecentHistory/GetRecentHistoryQuery.cs`, `GetRecentHistoryQueryHandler.cs` | `backend/src/TuneVault.Infrastructure/Repositories/PlayHistoryRepository.cs`; đọc `dbo.PlayHistory`, `dbo.MediaItems` |
| `POST /api/history/{mediaId}` | `frontend/src/React/App.tsx` -> `recordPlayHistory()` | `HistoryController.cs::RecordPlayHistoryByMediaId()` | `backend/src/TuneVault.Application/Features/History/Commands/RecordPlayHistory/RecordPlayHistoryCommand.cs`, `RecordPlayHistoryCommandHandler.cs` | `PlayHistoryRepository.cs`; ghi `dbo.PlayHistory` |
| `PATCH /api/history/{mediaId}/stop` | `App.tsx` -> `recordPlaybackStop()` | `HistoryController.cs::RecordPlaybackStop()` | `backend/src/TuneVault.Application/Features/History/DTOs/RecordPlaybackStopRequestDto.cs`, `backend/src/TuneVault.Application/Features/History/Commands/RecordPlayHistory/RecordPlayHistoryCommand.cs`, `RecordPlayHistoryCommandHandler.cs` | `PlayHistoryRepository.cs`; cập nhật `dbo.PlayHistory.StoppedAtSeconds` |
| `GET /api/history/{mediaId}/resume` | `App.tsx` -> `getResumeInfo()` | `HistoryController.cs::GetResumeInfo()` | `backend/src/TuneVault.Application/Features/History/Queries/GetHistoryResume/GetHistoryResumeQuery.cs`, `GetHistoryResumeQueryHandler.cs` | `PlayHistoryRepository.cs`; đọc `dbo.PlayHistory` |

## 10. Database schema files tham chiếu

Các file schema/init chính đang hỗ trợ các luồng trên:

- `backend/src/TuneVault.Infrastructure/Database/schemas/V1_TuneVault.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V2_AddMissingColumns.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V3_AddOtpLogs.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V7_AddFriendIsActive.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V8_AddCollectionLikes.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V9_ExtendFavoritesToAlbumPlaylistTargets.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V10_AddEnumCheckConstraints.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V12_AddFavoritesIsActiveSoftDelete.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V13_LoveOnlyFavorites.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V15_RestrictMediaTypesToAudioVideoSong.sql`
- `backend/src/TuneVault.Infrastructure/Database/schemas/V16_SplitMediaDurationMinutesSeconds.sql`
- `backend/src/TuneVault.Infrastructure/Database/init/init-db.sh`
- `backend/src/TuneVault.Infrastructure/Database/init/V14_EnsureCollectionLikes.sql`

## 11. File storage path liên quan

- Media, cover, poster, canvas, avatar: `backend/src/TuneVault.API/wwwroot/uploads`
- Các service xử lý file: `backend/src/TuneVault.Infrastructure/Services/LocalFileStorageService.cs`
- Các endpoint stream/poster chỉ trả URL file, không tự lưu binary vào DB.
