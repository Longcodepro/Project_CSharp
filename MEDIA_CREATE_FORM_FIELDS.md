# Media Create Form Fields

Tài liệu này tổng hợp đúng các field cần có khi tạo mới `media`, `video`, `podcard`, `album`, `playlist` theo contract hiện tại của TuneVault.

## Quy ước chung

- `Required`: bắt buộc nhập/chọn trong UI.
- `Optional`: có thể bỏ trống.
- `Auto`: frontend/backend tự set, không cho user nhập.
- `Hidden`: không hiển thị ở form cơ bản.

## 1. Media - Song

### Field nên hiển thị

| Field | Loại | Bắt buộc | Ghi chú |
|---|---:|---:|---|
| Title | text | Required | Tên bài hát. |
| Description | textarea | Optional | Mô tả ngắn. |
| Genre | text | Optional | Ví dụ: Pop, Electronic, R&B. |
| AudioFile | file | Required | Chọn file audio từ máy local. |
| CoverImage | file | Optional | Ảnh bìa bài hát. |
| CanvasFile | file | Optional | Canvas/video nền ngắn. |
| ReleaseDate | datetime-local | Optional | Default là thời điểm hiện tại, user có thể chỉnh. |
| IsPublic | switch | Optional | Default `true`. |

### Field không nên cho nhập

| Field | Loại | Trạng thái | Ghi chú |
|---|---:|---:|---|
| Owner / OwnerId | auto | Auto | Lấy theo user đang đăng nhập. |
| Type | hidden | Auto | Mặc định theo tab `Song`. |
| DurationSeconds | hidden | Auto | Frontend tự đọc duration từ file audio. |
| AccessLevel | hidden | Auto | Tự theo account tạo nội dung. |
| FeaturedArtistIds | advanced/hidden | Auto hoặc optional | Chỉ dùng khi muốn hỗ trợ cộng tác nghệ sĩ. Mặc định để rỗng. |

## 2. Media - Video

### Field nên hiển thị

| Field | Loại | Bắt buộc | Ghi chú |
|---|---:|---:|---|
| Title | text | Required | Tên video. |
| Description | textarea | Optional | Mô tả ngắn. |
| Genre | text | Optional | Thể loại video. |
| VideoFile | file | Required | Chọn file video từ máy local. |
| CoverImage | file | Optional | Ảnh bìa video. |
| ReleaseDate | datetime-local | Optional | Default là thời điểm hiện tại. |
| IsPublic | switch | Optional | Default `true`. |

### Field không nên cho nhập

| Field | Loại | Trạng thái | Ghi chú |
|---|---:|---:|---|
| Owner / OwnerId | auto | Auto | Lấy theo user đang đăng nhập. |
| Type | hidden | Auto | Mặc định theo tab `Video`. |
| DurationSeconds | hidden | Auto | Frontend tự đọc duration từ file video. |
| AccessLevel | hidden | Auto | Tự theo account tạo nội dung. |
| CanvasFile | hidden/không dùng | Hidden | Backend hiện không cho upload canvas riêng cho video. |
| FeaturedArtistIds | advanced/hidden | Auto hoặc optional | Nếu không cần collab thì để rỗng. |

## 3. Media - Podcard / Podcast / Audio

> Trong code hiện tại tab này đang dùng tên `Audio`, nhưng nghiệp vụ tương ứng với podcard/podcast.

### Field nên hiển thị

| Field | Loại | Bắt buộc | Ghi chú |
|---|---:|---:|---|
| Title | text | Required | Tên podcast / audio. |
| Description | textarea | Optional | Mô tả ngắn. |
| Genre | text | Optional | Ví dụ: Talkshow, Education, Chill. |
| AudioFile | file | Required | Chọn file audio từ máy local. |
| CoverImage | file | Optional | Ảnh bìa. |
| CanvasFile | file | Optional | Nếu muốn dùng visual canvas cho audio. |
| ReleaseDate | datetime-local | Optional | Default là thời điểm hiện tại. |
| IsPublic | switch | Optional | Default `true`. |

### Field không nên cho nhập

| Field | Loại | Trạng thái | Ghi chú |
|---|---:|---:|---|
| Owner / OwnerId | auto | Auto | Lấy theo user đang đăng nhập. |
| Type | hidden | Auto | Mặc định theo tab `Audio` / `Podcast`. |
| DurationSeconds | hidden | Auto | Frontend tự đọc duration từ file audio. |
| AccessLevel | hidden | Auto | Tự theo account tạo nội dung. |
| FeaturedArtistIds | advanced/hidden | Auto hoặc optional | Nếu có khách mời thì mới dùng. |

## 4. Album

### Field nên hiển thị

| Field | Loại | Bắt buộc | Ghi chú |
|---|---:|---:|---|
| Title | text | Required | Tên album. |
| Description | textarea | Optional | Mô tả album. |
| CoverImage | file | Optional | Ảnh bìa album. |
| ContentType | select | Optional | Có thể default `Song`. |
| ReleaseDate | datetime-local | Optional | Default là thời điểm hiện tại. |
| IsPublic | switch | Optional | Default `true`. |

### Field không nên cho nhập

| Field | Loại | Trạng thái | Ghi chú |
|---|---:|---:|---|
| Owner / ArtistId | auto | Auto | Lấy theo user đang đăng nhập. |
| Tracks | hidden ở form tạo mới | Auto | Track thường thêm sau khi tạo album xong. |

### Nếu muốn làm screen edit album

- Hiển thị danh sách track theo dòng.
- Mỗi dòng có:
  - ảnh + tên track
  - nút xóa
  - kéo-thả để đổi thứ tự
- Track order không phải field bắt buộc khi tạo mới album.

## 5. Playlist

### Field nên hiển thị

| Field | Loại | Bắt buộc | Ghi chú |
|---|---:|---:|---|
| Title | text | Required | Tên playlist. |
| Description | textarea | Optional | Mô tả playlist. |
| CoverImage | file | Optional | Ảnh bìa playlist. |
| ContentType | select | Optional | Có thể default `Song`. |
| ReleaseDate | datetime-local | Optional | Default là thời điểm hiện tại. |
| IsPublic | switch | Optional | Default `true`. |

### Field không nên cho nhập

| Field | Loại | Trạng thái | Ghi chú |
|---|---:|---:|---|
| Owner / OwnerId | auto | Auto | Lấy theo user đang đăng nhập. |
| Tracks | hidden ở form tạo mới | Auto | Track thường thêm sau khi tạo playlist xong. |

### Nếu muốn làm screen edit playlist

- Danh sách track render theo từng dòng.
- Mỗi dòng có:
  - nút xóa
  - kéo-thả đổi thứ tự
- Track order không phải field bắt buộc khi tạo mới playlist.

## 6. Gợi ý UI nên giữ / nên ẩn

### Nên giữ

- Title
- Description
- File upload đúng loại theo tab
- Cover image
- Release date
- Public toggle

### Nên ẩn hoặc tự động hóa

- Owner
- Type
- AccessLevel
- DurationSeconds
- FeaturedArtistIds nếu UI cơ bản

### Không nên bắt user nhập

- Duration của file
- ID hệ thống
- Path file
- Quyền truy cập nội bộ

## 7. Ghi chú nghiệp vụ

- `Video` nên chỉ nhận `VideoFile`, không nên cho upload `CanvasFile`.
- `Song` và `Podcard/Audio` nên đọc duration tự động từ file local.
- `ReleaseDate` nên default bằng thời điểm hiện tại và cho phép chỉnh bằng `datetime-local`.
- `IsPublic` nên để là toggle ở cuối form.
- `Owner` và `AccessLevel` nên để hệ thống tự set.
- Album/playlist nên tạo metadata trước, track management làm ở màn edit riêng hoặc ngay sau khi tạo xong.
