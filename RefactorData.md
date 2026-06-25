# RefactorData - Audit enum va database TuneVault

## 1. Pham vi da doc

- `AGENTS.md`
- `CURRENT_STATUS.md`
- `RUN_LOCAL.md`
- `API_CONTRACT.md`
- `docs/archive/DATABASE_SCHEMA.md`
- `Untitled.sql`
- `backend/src/TuneVault.Domain/Enums/*`
- Repository/Application/Controller lien quan mapping enum:
  - `FavoriteRepository.cs`
  - `CollectionLikeRepository.cs`
  - `MediaRepository.cs`
  - `MediaShareRepository.cs`
  - `NotificationRepository.cs`
  - `FriendRepository.cs`
  - cac command/DTO lien quan Friend, Share, Favorite, CollectionLike, Album, Playlist, Media
- Frontend mapping lien quan enum:
  - `frontend/src/React/App.jsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/Components/Sidebar.jsx`
  - `frontend/src/React/Components/ManageStudio.jsx`
  - `frontend/Services/MediaService.tsx`
- Migration/seed lien quan:
  - `V5_SeedData.sql`
  - `V6_AddBusinessRuleColumns.sql`
  - `V8_AddCollectionLikes.sql`
  - `V9_ExtendFavoritesToAlbumPlaylistTargets.sql`

## 2. Ket luan ngan

Database va codebase dang co nhieu concept enum bi lap o nhieu tang: DB `tinyint`/`varchar`, Domain enum, magic number trong Application/Infrastructure, string request/response DTO, va mapping frontend. Viec nay chua phai loi runtime o tat ca cho, nhung la rui ro lon vi chi can doi thu tu enum hoac chen enum moi la du lieu cu co the bi map sai.

Van de can uu tien nhat khong phai "co nhieu enum", ma la chua co single source of truth va chua co CHECK constraint day du trong DB. Hien tai phan lon cot `tinyint` co nghia enum nhung DB khong chan gia tri sai; code lai cast truc tiep sang enum o nhieu noi.

## 3. Bang doi chieu DB enum-like columns

| Bang/cot DB | Kieu DB | Code dang dung | Gia tri hien tai/du kien | Danh gia |
|---|---:|---|---|---|
| `MediaItems.MediaType` | `tinyint` | `MediaType` | `Audio=0`, `Video=1`, `Podcast=2`, `Song=3` | Dung duoc, nhung enum dang implicit value. Nen explicit value va them CHECK. |
| `Albums.ContentType` | `tinyint null` | `MediaType?` | cung `MediaType` | Dung chung concept voi media. Nen ghi ro `ContentType` la filter loai media, khong tao enum rieng neu khong can. |
| `Playlists.ContentType` | `tinyint null` | `MediaType?` | cung `MediaType` | Giong Albums. |
| `MediaItems.AccessLevel` | `tinyint` | `AccessLevel` | `Normal=0`, `Premium=1` | Dung duoc, nhung enum implicit. API chap nhan ca string va number. Nen explicit value va CHECK. |
| `Favorites.Reaction` | `tinyint` | `FavoriteReaction` | `Dislike=0`, `Like=1`, `Love=2`, `Chill=3`, `Energetic=4`, `Save=5`, `Remove=6` | Co rui ro: `Remove=6` la command action, khong nen luu DB. DB chua co CHECK chan `Remove`. |
| `Favorites.TargetType` | `tinyint` | `FavoriteTargetType` | `Media=0`, `Album=1`, `Playlist=2` | Dang khop voi `V9` va `Untitled.sql`. Nen giu explicit vi da co data. |
| `CollectionLikes.TargetType` | `tinyint` | `CollectionLikeTargetType` | `Album=1`, `Playlist=2` | Trung mot phan voi `Favorites.TargetType`. Can quyet dinh giu `CollectionLikes` hay hop nhat vao `Favorites`. |
| `Friends.Status` | `tinyint` | `FriendStatus`, nhung feature dung `byte` constants | `Pending=1`, `Accepted=2`, `Blocked=3` | Enum co ton tai nhung Application/Repository khong dung. Nen thay constants bang enum. |
| `MediaShares.ShareType` | `tinyint` | `ShareType`, nhung repository/handler dung string va magic number | `MediaItem=1`, `Album=2`, `Playlist=3` | Concept bi lap: string aliases `Track/Media/Song/Video`, enum, DB number. Nen gom mapping ve 1 cho. |
| `Notifications.NotifyType` | `tinyint` | `NotificationType`, nhung insert/select dung int va CASE | `NewFollower=1` den `ArtistNewMedia=6` | Enum khop nhung bi bypass boi `NotificationInsertModel.NotifyType int` va CASE hardcode. |
| `Notifications.TargetType` | `tinyint null` | Chua co enum rieng | Share handler dang dung `Track=1`, `Album=2`, `Playlist=3` | Khong nen dung lan voi `FavoriteTargetType` vi favorite co `Media=0`. Nen tao enum rieng. |
| `Ads.AdType` | `tinyint` | `AdType` | `AudioInterrupt=1`, `VideoRoll=2`, `Banner=3` | Enum co, nhung module Ads chua thay repository su dung ro. Nen them CHECK khi module Ads duoc dung. |
| `OtpLogs.Purpose` | `varchar(20)` | string magic | `register`, `reset_password`, `change_password` | DB va API_CONTRACT dang lech: contract ghi 2 gia tri, handler chap nhan 3. Nen tao constants/enum string va cap nhat contract. |
| `Admins.Role` | `varchar(50)` | string role | chua ro tap gia tri | Chua nen tao enum neu auth/role chua chot. Can xac nhan business rule. |
| `MediaArtists.Role` | `nvarchar(100)` | string role | default `MainArtist` | Khac concept voi Admin/Auth role. Nen giu string hoac tao value object rieng neu can role nghe si. |

## 4. Diem lech va duplication dang thay

### 4.1 `Untitled.sql` va `DATABASE_SCHEMA.md` dang lech

`Untitled.sql` hien tai da co `Favorites.TargetId`, `Favorites.TargetType`, `MediaItemId NULL`, index `IX_Favorites_User_Target`, va check `CK_Favorites_TargetType`.

`docs/archive/DATABASE_SCHEMA.md` van mo ta `Favorites` theo dang cu hon: `MediaItemId NOT NULL`, chua co `TargetId` va `TargetType`. Neu AI hoac dev dua vao file archive nay de sua repository se de viet sai query.

Khuyen nghi: sau khi chot schema, cap nhat lai `docs/archive/DATABASE_SCHEMA.md` tu `Untitled.sql` hoac tao file schema moi o root/docs active, tranh de archive cu lam nguon chinh.

### 4.2 `FavoriteTargetType` va `CollectionLikeTargetType` trung concept

Hien co 2 co che:

- `Favorites`: da duoc mo rong de react `Media`, `Album`, `Playlist`.
- `CollectionLikes`: chi like `Album`, `Playlist`.

Neu muc tieu UI la "reaction/cam xuc" cho album/playlist thi nen dung `Favorites` va bo dan `CollectionLikes`. Neu `CollectionLikes` la "save collection vao library" thi can doi ten concept de khong trung nghia voi favorite/reaction.

### 4.3 `ShareType`, `Notification.TargetType`, `FavoriteTargetType` deu noi ve target nhung numbering khac

- `FavoriteTargetType`: `Media=0`, `Album=1`, `Playlist=2`.
- `ShareType`: `MediaItem=1`, `Album=2`, `Playlist=3`.
- `Notifications.TargetType`: hien share handler dang gan `Track=1`, `Album=2`, `Playlist=3`.

Khong nen ep tat ca dung chung enum neu da co data. Nen tao enum rieng cho tung cot DB hoac tao enum chung moi nhung migration can ro rang. Cach an toan nhat:

- Giu `FavoriteTargetType` nhu hien tai vi DB da co constraint `0,1,2`.
- Dung `ShareType` cho `MediaShares.ShareType`.
- Tao `NotificationTargetType` cho `Notifications.TargetType` voi `Media=1`, `Album=2`, `Playlist=3`, va them comment ro khong phai `FavoriteTargetType`.

### 4.4 Magic number con rai trong Application/Infrastructure

Vi du:

- Friend commands/repository dung `PendingStatus = 1`, `AcceptedStatus = 2`.
- Share handler gan `NotifyType = 3`.
- Share handler gan `Track = 1`, `Album = 2`, `Playlist = 3`.
- Notification repository CASE `NotifyType` 1..6 de tra string.
- Frontend lap lai `numericMap = ['audio', 'video', 'podcast', 'song']` o nhieu component.

Nen thay bang enum/domain mapping helper de khi doi enum chi sua mot noi.

### 4.5 Namespace/style enum chua dong nhat

- `FriendStatus.cs` nam trong folder `Domain/Enums` nhung namespace la `TuneVault.Domain.Entities`.
- `ShareType.cs` khong co namespace.
- `MediaType` va `AccessLevel` dang implicit value.

Day la no ky thuat nho nhung nguy hiem khi refactor/using. Nen chuan hoa ve `TuneVault.Domain.Enums` va explicit value.

### 4.6 DB thieu CHECK constraint cho nhieu cot enum

Hien DB chi thay constraint ro cho:

- `Favorites.TargetType` trong `Untitled.sql`.
- `CollectionLikes.TargetType` trong migration `V8`.

Nen bo sung constraint cho:

- `MediaItems.MediaType IN (0,1,2,3)`
- `MediaItems.AccessLevel IN (0,1)`
- `Albums.ContentType IS NULL OR IN (0,1,2,3)`
- `Playlists.ContentType IS NULL OR IN (0,1,2,3)`
- `Favorites.Reaction IN (0,1,2,3,4,5)` neu `Remove` chi la command action, khong luu DB.
- `Friends.Status IN (1,2,3)`
- `MediaShares.ShareType IN (1,2,3)`
- `Notifications.NotifyType IN (1,2,3,4,5,6)`
- `Notifications.TargetType IS NULL OR IN (1,2,3)` neu chot target chi gom media/album/playlist.
- `Ads.AdType IN (1,2,3)`
- `OtpLogs.Purpose IN ('register','reset_password','change_password')` neu SQL Server check string duoc chap nhan trong convention project.

## 5. Rui ro neu sua ngay khong co plan

- Doi thu tu enum implicit co the lam toan bo du lieu `tinyint` bi doc sai.
- Hop nhat `CollectionLikes` vao `Favorites` co the lam mat behavior sidebar/library neu frontend dang goi `/collection-likes/recent`.
- Doi response enum tu string sang number hoac nguoc lai co the lam frontend hong filter media, reaction, notification.
- Them CHECK constraint khi DB dang co data sai se fail migration. Can query kiem tra truoc.
- `Notification.TargetType` va `FavoriteTargetType` trung ten nhung khac numbering; dung nham enum se tao notification target sai.

## 6. Plan refactor de xuat

### Buoc 1 - Dong bang contract enum

Muc tieu: co 1 tai lieu canonical truoc khi sua code.

File du kien sua:

- `API_CONTRACT.md`
- Tao moi `docs/ENUM_CONTRACT.md` hoac bo sung vao `API_CONTRACT.md`

Noi dung can chot:

- Gia tri numeric cua tung enum luu DB.
- Request/response API dung string hay number cho tung field.
- `Favorites` va `CollectionLikes` co giu song song hay hop nhat.

Can hoi developer:

- `CollectionLikes` co phai duplicate voi `Favorites` khong, hay no la "save collection" rieng?
- `FavoriteReaction.Remove` co duoc phep luu trong DB khong, hay chi la action de delete row?
- `OtpLogs.Purpose` co chinh thuc them `change_password` khong?

### Buoc 2 - Chuan hoa Domain enum

Muc tieu: enum trong Domain la single source trong backend.

File du kien sua:

- `backend/src/TuneVault.Domain/Enums/MediaType.cs`
- `backend/src/TuneVault.Domain/Enums/AccessLevel.cs`
- `backend/src/TuneVault.Domain/Enums/FriendStatus.cs`
- `backend/src/TuneVault.Domain/Enums/ShareType.cs`
- Tao moi `backend/src/TuneVault.Domain/Enums/NotificationTargetType.cs`

Noi dung sua:

- Dat explicit numeric value cho tat ca enum luu DB.
- Dua `FriendStatus` ve namespace `TuneVault.Domain.Enums`.
- Them namespace cho `ShareType`.
- Tao enum rieng cho `Notifications.TargetType` neu tiep tuc luu target type.

Kiem tra:

- `dotnet build`

### Buoc 3 - Thay magic number trong Application/Infrastructure

Muc tieu: Application khong con hardcode enum number.

File du kien sua:

- `FriendRepository.cs`
- `AcceptFriendRequestCommand.cs`
- `CancelFriendRequestCommand.cs`
- `RejectFriendRequestCommand.cs`
- `RemoveFriendCommand.cs`
- `SendFriendRequestCommand.cs`
- `ShareMediaCommandHandler.cs`
- `MediaShareRepository.cs`
- `NotificationRepository.cs`
- `NotificationInsertModel.cs`
- cac query/DTO notification neu can

Noi dung sua:

- Doi `byte Status` constants sang `FriendStatus`.
- Doi `NotifyType = 3` sang `NotificationType.MediaShared`.
- Doi `TargetType = 1/2/3` sang `NotificationTargetType`.
- Doi `ToShareType` ve enum `ShareType`, chi de alias parsing o mot helper duy nhat.
- Notification select khong hardcode CASE duplicate neu co the dung mapping o Application; neu van can SQL CASE thi comment va dong bo voi enum.

Kiem tra:

- `dotnet build`
- Test nhanh cac endpoint friends/share/notifications neu backend chay duoc.

### Buoc 4 - Them mapping/validation helper khi doc DB

Muc tieu: khong cast enum tu DB mot cach im lang khi data sai.

File du kien sua:

- Tao moi helper, vi du `backend/src/TuneVault.Infrastructure/Repositories/EnumDbMapper.cs`
- `MediaRepository.cs`
- `FavoriteRepository.cs`
- `CollectionLikeRepository.cs`
- `AlbumRepository.cs`
- `PlaylistRepository.cs`
- `NotificationRepository.cs`

Noi dung sua:

- Khi doc `tinyint` tu DB, validate `Enum.IsDefined`.
- Neu data sai, tra loi error ro hoac fallback co kiem soat tuy endpoint.
- Khi ghi DB, luon cast enum sang `byte/int` o repository, khong gui raw string/number tu controller vao SQL.

Kiem tra:

- `dotnet build`
- Chay query detect invalid enum truoc/sau migration.

### Buoc 5 - Bo sung SQL constraints sau khi quet data

Muc tieu: DB chan data enum sai tu goc.

File du kien sua:

- Tao migration moi trong `backend/src/TuneVault.Infrastructure/Database/schemas/`, vi du `V10_AddEnumCheckConstraints.sql`
- Cap nhat schema docs sau khi chay xong.

Truoc khi add constraint, chay cac query audit:

```sql
SELECT * FROM MediaItems WHERE MediaType NOT IN (0,1,2,3);
SELECT * FROM MediaItems WHERE AccessLevel NOT IN (0,1);
SELECT * FROM Albums WHERE ContentType IS NOT NULL AND ContentType NOT IN (0,1,2,3);
SELECT * FROM Playlists WHERE ContentType IS NOT NULL AND ContentType NOT IN (0,1,2,3);
SELECT * FROM Favorites WHERE Reaction NOT IN (0,1,2,3,4,5);
SELECT * FROM Favorites WHERE TargetType NOT IN (0,1,2);
SELECT * FROM Friends WHERE Status NOT IN (1,2,3);
SELECT * FROM MediaShares WHERE ShareType NOT IN (1,2,3);
SELECT * FROM Notifications WHERE NotifyType NOT IN (1,2,3,4,5,6);
SELECT * FROM Notifications WHERE TargetType IS NOT NULL AND TargetType NOT IN (1,2,3);
SELECT * FROM Ads WHERE AdType NOT IN (1,2,3);
SELECT * FROM OtpLogs WHERE Purpose NOT IN ('register','reset_password','change_password');
```

Kiem tra:

- Chay migration tren DB dev.
- `dotnet build`
- Endpoint smoke test cac module bi anh huong.

### Buoc 6 - Quyet dinh `CollectionLikes` vs `Favorites`

Option A - Khuyen nghi neu muon reaction thong nhat:

- Dung `Favorites` lam bang duy nhat cho media/album/playlist reaction.
- Deprecate `/collection-likes` hoac doi thanh wrapper goi `Favorites`.
- Migration copy data `CollectionLikes` sang `Favorites` voi `Reaction=Like`, tranh duplicate bang unique index.
- Sau khi frontend khong con dung, moi xem xet drop `CollectionLikes`.

Option B - Neu `CollectionLikes` la save collection rieng:

- Doi ten UI/API thanh `saved-collections` hoac document ro "like collection" khac "favorite reaction".
- Giu `CollectionLikeTargetType`.
- Khong tron count/reaction cua `Favorites` voi saved collections.

Can developer chot truoc khi code.

### Buoc 7 - Gom frontend enum mapping

Muc tieu: frontend khong copy `numericMap` o nhieu component.

File du kien sua:

- Tao moi `frontend/src/React/utils/enumMaps.js` hoac gan trong service layer.
- `App.jsx`
- `Home.jsx`
- `Sidebar.jsx`
- `ManageStudio.jsx`
- `MediaService.tsx`

Noi dung sua:

- Tao `normalizeMediaType(value)` dung chung.
- Tao `normalizeTargetType(value)` neu frontend can hien album/playlist/media.
- Uu tien backend tra string display name cho UI; frontend chi fallback number khi can tuong thich data cu.
- `getFavoriteReactions()` da expose enum reaction; UI nen dung endpoint nay thay vi hardcode list reaction.

Kiem tra:

- `npm run build`
- `npm run dev`

### Buoc 8 - Cap nhat docs active

File du kien sua:

- `API_CONTRACT.md`
- `docs/archive/DATABASE_SCHEMA.md` hoac schema active moi
- `CURRENT_STATUS.md` neu thay doi lon
- `AI_CHANGELOG.md` sau khi refactor xong

Noi dung sua:

- Ghi ro enum values.
- Ghi ro endpoint nao nhan string, endpoint nao tra string/number.
- Cap nhat `Favorites` schema theo `Untitled.sql` hien tai.
- Cap nhat `OtpLogs.Purpose` neu co `change_password`.

## 7. Thu tu uu tien khuyen nghi

1. Chot contract enum va quyet dinh `CollectionLikes` vs `Favorites`.
2. Explicit value + namespace cho Domain enum.
3. Thay magic number backend.
4. Them CHECK constraints sau khi quet data.
5. Gom mapping frontend.
6. Cap nhat docs.

## 8. File chi nen doc, khong sua trong refactor neu khong can

- `Untitled.sql`: chi la snapshot schema hien tai, khong nen sua truc tiep neu project dung migration script rieng.
- `docs/archive/*`: chi sua khi can cap nhat tai lieu tham chieu; khong dung archive lam migration source.
- File UI khong lien quan enum/media/favorite/share/notification.

## 9. Cach kiem tra sau refactor

- Backend:
  - `dotnet build backend/src/TuneVault.sln`
  - Smoke test favorites media/album/playlist.
  - Smoke test collection-likes neu con giu.
  - Smoke test friend request lifecycle.
  - Smoke test share media/album/playlist va notification sinh ra.
- Database:
  - Chay query invalid enum truoc migration.
  - Chay migration constraints.
  - Chay lai query invalid enum sau migration.
- Frontend:
  - `npm run build` trong `frontend`
  - `npm run dev` trong `frontend`
  - Kiem tra Home filter media type, ManageStudio create/edit media, Sidebar saved/liked items, Notifications.

## 10. Ghi chu hien trang workspace

- Worktree dang co rat nhieu thay doi/xoa file cu `src/` va `client/`, dong thoi co thu muc moi `backend/` va `frontend/`.
- `RUN_LOCAL.md` van nhac `src/` va `client/`, trong khi code thuc te dang nam o `backend/` va `frontend/`.
- Neu refactor that, can thao tac tren `backend/` va `frontend/`, khong phai `src/`/`client/` cu.

