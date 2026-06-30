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

## Cấu hình password database -> phải cấu hình thì mới chạy seed data bằng lệnh buid docker luôn

Điền các biến sau:

```env
SQL_SERVER_PASSWORD= password
```

## Chạy bằng Docker và tạo seed data

Lần đầu chạy local:

```bash
docker compose up -d --build
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
docker compose down -v
docker compose up -d --build
```

Kiểm tra nhanh container:

```bash
docker compose ps
```

Swagger UI:

```txt
http://localhost:5128/swagger
```

## Chỉ chạy lại backend + frontend khi SQL Server đã có sẵn

Nếu SQL Server đã chạy sẵn rồi, bạn chỉ cần dựng lại `backend` và `frontend`. Trước hết xóa 2 container app cũ nếu chúng còn tồn tại:

```bash
docker rm -f tunevault_backend tunevault_frontend
```

Sau đó chạy:

```bash
DB_HOST=host.docker.internal DB_PORT=1433 DB_NAME=TuneVaultDb DB_USER=sa docker compose up -d --build --no-deps backend frontend
```

Nếu SQL Server của bạn nằm chung network với compose hiện tại thì có thể bỏ `DB_HOST=host.docker.internal`, còn mặc định chạy qua host machine là cách an toàn nhất khi DB đã được dựng từ project khác.

Lưu ý:

- Không dùng `docker compose down -v` nếu không muốn xóa volume database.
- `--no-deps` giúp Compose không cố tạo lại service `db` trong file hiện tại.
- Script `db-init` có kiểm tra schema và seed data trước khi chạy, nên nếu database đã có sẵn thì sẽ tự bỏ qua các bước init không cần thiết.

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

## Ghi chú seed assets -> phải có wwwroot thì mới có dữ liệu khi chạy local được

Seed data đang trỏ tới file trong `backend/src/TuneVault.API/wwwroot/uploads`. Không xóa các file trong `uploads/default-cover` vì đây là danh sách ảnh bìa mặc định cho song/audio/video/playlist/album.

Thư mục `uploads` hiện gồm:

- `avatars`: avatar user seed.
- `media`: file mp3 seed.
- `media-covers`: ảnh bìa media seed.
- `canvas`: video canvas seed.
- `default-cover`: 13 ảnh bìa mặc định.


docker compose up -d --build --no-deps backend frontend
