## Cấu trúc nộp bài

- `backend/src/TuneVault.sln`: solution .NET.
- `backend/src/TuneVault.API`: ASP.NET Core API, Swagger, static uploads.
- `backend/src/TuneVault.Application`: CQRS/MediatR, DTO, business handlers.
- `backend/src/TuneVault.Domain`: entity, enum, interface domain.
- `backend/src/TuneVault.Infrastructure`: Dapper repositories, auth, storage, realtime, DB init.
- `frontend`: source React/Vite.
- `database.sql`: schema snapshot cho Docker DB init.
- `seed.sql`: seed data và tài khoản mẫu.
- `docs/ERD.pdf`: ERD.
- `docs/PipeLine.pdf`: pipeline.
- `docs/swagger.json`: Swagger export.

## Yêu cầu môi trường

- Docker Desktop hoặc Docker Engine + Docker Compose.
- .NET SDK 9 nếu chạy backend bằng CLI.
- Node.js 20 nếu chạy frontend bằng CLI.
- SQL Server chỉ cần cài riêng khi không dùng Docker.

## Cấu hình password database

Điền các biến sau:

```env
SQL_SERVER_PASSWORD=YOUR_SQL_PASSWORD
```

## Chạy bằng Docker và tạo seed data

Lần đầu chạy local:

```bash
docker compose -f docker-compose.local.yml up -d --build
```

Docker sẽ tự làm các bước:

- Tạo container SQL Server `sqlserver`.
- Tạo database `TuneVaultDb` nếu chưa có.
- Chạy `database.sql` nếu schema chưa có bảng `Users`.
- Chạy `seed.sql` nếu seed user `U003` chưa tồn tại.
- Build và chạy backend tại `http://localhost:5128`.
- Build và chạy frontend tại `http://localhost:3000`.

Nếu muốn tạo lại database và seed data từ đầu: dành cho việc test lại

```bash
docker compose -f docker-compose.local.yml down -v
docker compose -f docker-compose.local.yml up -d --build
```

Kiểm tra nhanh container:

```bash
docker compose -f docker-compose.local.yml ps
```

Swagger UI:

```txt
http://localhost:5128/swagger
```

## Tài khoản seed: nhập idDisplay + password => đăng nhập được hoặc có thể tạo một user mới luôn cũng được

Quy ước password: `idDisplay + 123`.

| Vai trò | IdDisplay | Password |
| --- | --- | --- |
| Listener | listener_one | listener_one123 |
| Listener | listener_two | listener_two123 |
| Artist | sontungmtp | sontungmtp123 |
| Artist | justinbieber | justinbieber123 |
| Artist | shakira | shakira123 |
| Artist | mono | mono123 |
| Artist | soobin | soobin123 |

## Swagger export

Khi backend đang chạy ở port `5128`, xuất Swagger:

```bash
curl -s http://localhost:5128/swagger/v1/swagger.json -o docs/swagger.json
```

File `docs/swagger.json` được dùng làm Swagger export trong gói nộp bài.

## Ghi chú seed assets

Seed data đang trỏ tới file trong `backend/src/TuneVault.API/wwwroot/uploads`. Thư mục này không đưa lên Git vì chứa media demo khá nặng; khi chạy demo đầy đủ thì dùng bộ assets đi kèm gói nộp bài hoặc copy lại từ môi trường local.

Thư mục `uploads` hiện gồm:

- `avatars`: avatar user seed.
- `media`: file mp3 seed.
- `media-covers`: ảnh bìa media seed.
- `canvas`: video canvas seed.
- `default-cover`: 13 ảnh bìa mặc định.
