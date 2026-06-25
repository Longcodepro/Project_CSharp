# Team Branch Push Guide

Tài liệu này là bản rút gọn để cả nhóm dùng chung khi chia code và push theo từng nhánh.

## 1. Thứ tự push ưu tiên

1. `CleanArchitecture`
2. `feature/user-auth-and-authorization`
3. `feature/album-playlist-search-dao`
4. `interaction-share-notification`
5. `feature/QuocTai-frontend`
6. `main` chỉ merge cuối cùng sau khi các nhánh feature ổn

## 1.1. Vì sao plan chỉ có 5 nhánh

Khi chạy `git branch -a`, các dòng trong danh sách không phải đều là branch độc lập.

- `CleanArchitecture`, `feature/user-auth-and-authorization`, `main` là branch local đang có.
- `remotes/origin/CleanArchitecture`
- `remotes/origin/feature/QuocTai-frontend`
- `remotes/origin/feature/album-playlist-search-dao`
- `remotes/origin/feature/user-auth-and-authorization`
- `remotes/origin/interaction-share-notification`
- `remotes/origin/main`
- `remotes/origin/HEAD -> origin/main`

Phần `remotes/origin/...` là remote-tracking ref, không phải branch mới để chia plan riêng. Vì vậy file này gom theo **5 nhánh chức năng thật**:

1. `CleanArchitecture`
2. `feature/user-auth-and-authorization`
3. `feature/album-playlist-search-dao`
4. `interaction-share-notification`
5. `feature/QuocTai-frontend`

Nếu team muốn, branch local nào chưa có thì tạo từ remote tương ứng trước khi commit.

## 2. Quy tắc chung khi tách commit

- Mỗi commit chỉ nên mang một mục tiêu rõ ràng.
- Backend và frontend chỉ đi chung commit khi đó là thay đổi contract end-to-end.
- Không trộn auth, album, playlist, share, notification và frontend chung một commit nếu tách được.
- Mỗi commit nên build pass trước khi push tiếp.
- Nếu file đang lẫn nhiều feature, ưu tiên `git add -p` hoặc cherry-pick sang branch đúng scope.

## 3. Kế hoạch commit theo từng nhánh

### `CleanArchitecture`

#### Commit 1

- Tên gợi ý: `chore: sync clean architecture foundation`
- File/folder push:
  - `backend/src/TuneVault.Application/Common/*`
  - `backend/src/TuneVault.Application/Abstractions/*`
  - `backend/src/TuneVault.Application/Interfaces/*`
  - `backend/src/TuneVault.Domain/Interfaces/*`

#### Commit 2

- Tên gợi ý: `chore: align repository and mapping helpers`
- File/folder push:
  - `backend/src/TuneVault.Infrastructure/Persistence/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/RepositoryMappingHelper.cs`

#### Commit 3

- Tên gợi ý: `chore: finalize api bootstrap and docs`
- File/folder push:
  - `backend/src/TuneVault.API/Program.cs`
  - `backend/src/TuneVault.API/Controllers/BaseApiController.cs`
  - các file tài liệu liên quan nếu có thay đổi bootstrap/DI

### `feature/user-auth-and-authorization`

#### Commit 1

- Tên gợi ý: `feat(auth): implement login register refresh and password flow`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/AuthController.cs`
  - `backend/src/TuneVault.API/DTOs/Users/*`
  - `backend/src/TuneVault.Application/Features/Auth/*`
  - `backend/src/TuneVault.Infrastructure/Authentication/JwtTokenGenerator.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/OtpLogRepository.cs`

#### Commit 2

- Tên gợi ý: `feat(user): stabilize profile follow and authorization`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/UsersController.cs`
  - `backend/src/TuneVault.Application/Features/User/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/FollowRepository.cs`

#### Commit 3

- Tên gợi ý: `fix(frontend-auth): sync cookie session and profile state`
- File/folder push:
  - `frontend/src/React/App.jsx`
  - `frontend/src/React/App.tsx`
  - `frontend/src/React/Components/AuthLoginModal.jsx`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/CSS/App_style.css`
  - `frontend/src/CSS/Login_style.css`
  - `frontend/src/CSS/PlayerBar.css`

### `feature/album-playlist-search-dao`

#### Commit 1

- Tên gợi ý: `feat(album): implement album create update and track ordering`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/AlbumsController.cs`
  - `backend/src/TuneVault.API/DTOs/Albums/*`
  - `backend/src/TuneVault.Application/Features/Album/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/AlbumRepository.cs`

#### Commit 2

- Tên gợi ý: `feat(playlist): align playlist queries and management`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/PlaylistController.cs`
  - `backend/src/TuneVault.API/DTOs/Playlists/*`
  - `backend/src/TuneVault.Application/Features/Playlist/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/PlaylistRepository.cs`

#### Commit 3

- Tên gợi ý: `feat(media-search): stabilize media browse and search contracts`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/SearchController.cs`
  - `backend/src/TuneVault.API/Controllers/MediaController.cs`
  - `backend/src/TuneVault.API/DTOs/Media/*`
  - `backend/src/TuneVault.Application/Features/Search/*`
  - `backend/src/TuneVault.Application/Features/Media/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/MediaRepository.cs`

### `interaction-share-notification`

#### Commit 1

- Tên gợi ý: `feat(friend): stabilize request lifecycle`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/FriendsController.cs`
  - `backend/src/TuneVault.Application/Features/Friend/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/FriendRepository.cs`

#### Commit 2

- Tên gợi ý: `feat(notification-share): connect notification and share flow`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/ShareController.cs`
  - `backend/src/TuneVault.API/Controllers/NotificationController.cs`
  - `backend/src/TuneVault.Application/Features/Share/*`
  - `backend/src/TuneVault.Application/Features/Notification/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/MediaShareRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/NotificationRepository.cs`

#### Commit 3

- Tên gợi ý: `feat(interaction): polish collection likes and realtime hooks`
- File/folder push:
  - `backend/src/TuneVault.API/Controllers/CollectionLikesController.cs`
  - `backend/src/TuneVault.Application/Features/CollectionLike/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/CollectionLikeRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Realtime/NotificationHub.cs`
  - `backend/src/TuneVault.Infrastructure/Realtime/SignalRNotificationPusher.cs`
  - `backend/src/TuneVault.API/Controllers/HistoryController.cs`
  - `backend/src/TuneVault.Application/Features/History/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/PlayHistoryRepository.cs`

### `feature/QuocTai-frontend`

#### Commit 1

- Tên gợi ý: `feat(frontend-shell): stabilize app bootstrap and navigation`
- File/folder push:
  - `frontend/src/main.jsx`
  - `frontend/src/React/App.jsx`
  - `frontend/src/CSS/index.css`
  - `frontend/src/CSS/Home.css`
  - `frontend/src/CSS/Sidebar.css`

#### Commit 2

- Tên gợi ý: `feat(frontend-auth-profile): sync auth modal and profile views`
- File/folder push:
  - `frontend/src/React/Components/AuthLoginModal.jsx`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/src/React/Components/Sidebar.jsx`
  - `frontend/src/CSS/Login_style.css`
  - `frontend/src/CSS/App_style.css`

#### Commit 3

- Tên gợi ý: `feat(frontend-library): integrate media player and side panels`
- File/folder push:
  - `frontend/src/React/App.tsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/Components/FriendActivity.jsx`
  - `frontend/src/React/Components/NotificationActivity.jsx`
  - `frontend/src/React/Components/PlayerBar.jsx`
  - `frontend/src/React/Components/PlaylistModal.jsx`
  - `frontend/src/React/Components/MediaInfoPanel.jsx`
  - `frontend/src/React/Components/VideoPlayerView.jsx`
  - `frontend/src/React/Components/ManageStudio.jsx`
  - `frontend/src/CSS/PlayerBar.css`
  - `frontend/src/CSS/FriendActivity.css`
  - `frontend/src/CSS/NotificationActivity.css`
  - `frontend/src/CSS/MediaInfoPanel.css`
  - `frontend/src/CSS/NowPlayingView.css`
  - `frontend/src/CSS/PlaylistModal.css`
  - `frontend/src/CSS/VideoPlayerView.css`
  - `frontend/Services/MediaService.tsx`

## 4. Checklist trước khi push

1. `git status` chỉ còn file đúng scope branch.
2. Commit message nói rõ feature/fix.
3. Backend branch thì chạy `dotnet build backend/src/TuneVault.sln`.
4. Frontend branch thì chạy `npm run build` trong `frontend`.
5. Có đổi endpoint/request/response thì cập nhật `API_CONTRACT.md`.
6. Có đổi trạng thái module lớn thì cập nhật `CURRENT_STATUS.md`.
7. Xong một batch lớn thì ghi `AI_CHANGELOG.md`.

## 5. Cách dùng nhanh

1. Chọn đúng branch theo chức năng.
2. Tách file theo commit tương ứng ở trên.
3. Commit từng batch nhỏ đúng file list.
4. Build test trước khi push.
5. Push branch xong mới chuyển sang nhánh kế tiếp.
