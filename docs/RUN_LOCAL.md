# Cách chạy TuneVault local

Tài liệu này bám theo codebase hiện tại của project TuneVault:

- Backend nằm trong `backend/src`
- API project là `backend/src/TuneVault.API`
- Frontend nằm trong `frontend`
- File Compose local mặc định là `docker-compose.yml`

`docker-compose.local.yml` hiện không tồn tại trong repo.

## 1. Chuẩn bị môi trường

Cần có:

- .NET SDK 9 vì backend target `net9.0`
- Node.js và npm để chạy frontend; Dockerfile frontend đang dùng `node:20-alpine`
- Docker Engine/Desktop kèm Docker Compose nếu muốn chạy SQL Server bằng container hoặc chạy full stack bằng Docker
- SQL Server local hoặc SQL Server Docker

Lưu ý cấu hình:

- Backend không tự load file `.env` khi chạy bằng `dotnet run`
- Docker Compose có đọc file `.env` ở thư mục gốc repo
- File `.env.example` đang dùng cú pháp `KEY=VALUE` chuẩn
- Nếu bạn dùng file `.env` riêng, nên giữ đúng cú pháp shell-compatible: không có khoảng trắng quanh `=`, giá trị boolean nên là `true` hoặc `false`

## 2. Cấu trúc thư mục quan trọng

```text
.
├── backend/
│   └── src/
│       ├── TuneVault.sln
│       └── TuneVault.API/
├── frontend/
├── .env
├── .env.example
└── docker-compose.yml
```

## 3. Cách 1: Chạy local bằng lệnh CLI

### 3.1. Chuẩn bị file môi trường

Từ thư mục gốc repo:

```bash
cp .env.example .env
```

Sau đó sửa giá trị trong `.env` theo máy local của bạn.

Các biến có trong `.env.example` hiện tại:

```env
SQL_SERVER_PASSWORD=change-me-local-only
JWT_SECRET=change-me-to-a-long-random-secret
ANTHROPIC_API_KEY=
EMAIL_SENDER_EMAIL=your-email@example.com
EMAIL_SENDER_PASSWORD=your-email-app-password
EMAIL_DEV_MODE=false
```

### 3.2. Chạy database local

Repo hiện đã có sẵn luồng Docker để chạy SQL Server và khởi tạo `TuneVaultDb`, nên đây là cách local dễ khớp codebase nhất:

```bash
docker compose up -d db db-init
docker compose ps -a
docker compose logs db-init
```

Thông tin thực tế từ `docker-compose.yml`:

- Service SQL Server là `db`
- Service khởi tạo schema/seed là `db-init`
- SQL Server publish ra host tại `localhost:1433`
- `db-init` là one-shot container; trạng thái `Exited (0)` sau khi chạy xong là bình thường

Nếu bạn không dùng Docker cho database, cần tự chuẩn bị SQL Server local, tạo database `TuneVaultDb`, rồi tự chạy `database.sql` và `seed.sql`. Repo hiện không có script CLI riêng cho trường hợp này; flow được đóng gói sẵn trong `db-init`.

### 3.3. Nạp biến môi trường cho backend

Backend hiện đọc config qua `appsettings.json`, `appsettings.Development.json` và environment variables của process. Nó không tự đọc `.env`.

Vì vậy, nếu muốn dùng `.env` khi chạy `dotnet run`, bạn cần nạp file này vào shell trước rồi ánh xạ sang các key mà ASP.NET Core đang đọc:

```bash
set -a
source .env
set +a

export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=TuneVaultDb;User Id=sa;Password=${SQL_SERVER_PASSWORD};TrustServerCertificate=True;"
export DatabaseOptions__ConnectionString="$ConnectionStrings__DefaultConnection"
export JwtSettings__SecretKey="$JWT_SECRET"
export JwtSettings__Issuer="TuneVault_Backend_API"
export JwtSettings__Audience="TuneVault_Client_Application"
export EmailSettings__DevMode="true"
```

Giải thích ngắn:

- Backend runtime hiện dùng `DatabaseOptions:ConnectionString`
- `JwtSettings:SecretKey`, `JwtSettings:Issuer`, `JwtSettings:Audience` là bắt buộc
- `EmailSettings__DevMode=true` giúp luồng OTP ghi log thay vì buộc phải cấu hình SMTP thật

Nếu muốn test gửi email thật, cần export thêm đúng key mà backend đang đọc:

```bash
export EmailSettings__SenderEmail="$EMAIL_SENDER_EMAIL"
export EmailSettings__SenderPassword="$EMAIL_SENDER_PASSWORD"
export EmailSettings__SmtpHost="smtp.gmail.com"
export EmailSettings__SmtpPort="587"
export EmailSettings__DevMode="false"
```

### 3.4. Chạy backend

```bash
cd backend/src
dotnet restore TuneVault.sln
dotnet build TuneVault.sln
dotnet run --project TuneVault.API/TuneVault.API.csproj --launch-profile http
```

Thông tin thực tế từ `launchSettings.json`:

- Profile `http` chạy tại `http://localhost:5128`
- Profile `https` còn khai báo thêm `https://localhost:7263`
- Hướng dẫn này dùng profile `http` để khớp với Vite proxy của frontend

### 3.5. Chạy frontend

Mở terminal khác từ thư mục gốc repo:

```bash
cd frontend
npm ci
npm run dev
```

Thông tin thực tế từ `package.json` và `vite.config.js`:

- Frontend dev server chạy tại `http://localhost:3000`
- Vite proxy các route sau sang backend `http://localhost:5128`
  - `/api`
  - `/uploads`
  - `/hubs`

### 3.6. Địa chỉ local sau khi chạy CLI

| Thành phần | Địa chỉ |
|---|---|
| Frontend | `http://localhost:3000` |
| Backend | `http://localhost:5128` |
| Swagger | `http://localhost:5128/swagger` |
| Health | `http://localhost:5128/health` |
| SQL Server | `localhost:1433` |

## 4. Cách 2: Chạy local bằng Docker Compose

### 4.1. File Compose đang dùng

Repo hiện có hai file Compose:

| File | Mục đích |
|---|---|
| `docker-compose.yml` | Local/full stack; publish SQL `1433`, backend `5128`, frontend `3000` |
| `docker-compose.pro.yml` | Cấu hình hướng deploy; frontend publish port `80`, backend và SQL chỉ nằm trong Docker network |

Để chạy local đầy đủ, dùng `docker-compose.yml`.

### 4.2. Chuẩn bị `.env`

```bash
cp .env.example .env
```

Sau đó sửa ít nhất các biến sau:

- `SQL_SERVER_PASSWORD`
- `JWT_SECRET`

Lưu ý thực tế của repo hiện tại:

- Docker Compose có map `SQL_SERVER_PASSWORD` vào SQL Server, `db-init`, `ConnectionStrings__DefaultConnection` và `DatabaseOptions__ConnectionString`
- Docker Compose có map `JWT_SECRET` vào `JwtSettings__SecretKey`
- Docker Compose có map `ANTHROPIC_API_KEY` vào `Anthropic__ApiKey`
- Docker Compose hiện chưa map các biến `EMAIL_*` sang `EmailSettings__*`

### 4.3. Chạy full stack local

```bash
docker compose up -d --build
docker compose ps -a
```

Theo dõi log nếu cần:

```bash
docker compose logs -f backend
```

```bash
docker compose logs -f frontend
```

```bash
docker compose logs -f db-init
```

Các cổng publish thực tế từ `docker-compose.yml`:

| Thành phần | Địa chỉ |
|---|---|
| Frontend | `http://localhost:3000` |
| Backend | `http://localhost:5128` |
| Swagger | `http://localhost:5128/swagger` |
| SQL Server | `localhost:1433` |

### 4.4. Dừng stack

```bash
docker compose down
```

Nếu muốn xóa luôn volume database local:

```bash
docker compose down -v
```

## 5. Khác biệt giữa CLI local và Docker local

| Nội dung | CLI local | Docker Compose local |
|---|---|---|
| `.env` có được đọc tự động không | Không | Có |
| Backend chạy ở đâu | `dotnet run` trên host | Container `backend` |
| Frontend chạy ở đâu | `vite` trên host | Container `frontend` qua Nginx |
| Database khởi tạo bằng gì | Bạn tự chuẩn bị hoặc dùng `db` + `db-init` | `db` + `db-init` |
| Mapping `EMAIL_*` sang backend | Phải export tay sang `EmailSettings__*` | Chưa được compose map sẵn |

## 6. Lệnh kiểm tra nhanh sau khi chạy

CLI local:

```bash
curl http://localhost:5128/health
curl http://localhost:5128/swagger/v1/swagger.json
```

Docker Compose local:

```bash
docker compose ps
docker compose logs --tail 100 backend
docker compose logs --tail 100 db-init
```

## 7. Ghi chú quan trọng

- `appsettings.Development.json` hiện chỉ chứa cấu hình logging; không chứa connection string, JWT hay email
- `appsettings.json` vẫn đang có placeholder cho DB/JWT và còn chứa `EmailSettings`
- Backend local hiện không dùng trực tiếp các tên biến `JWT_SECRET`, `EMAIL_SENDER_EMAIL`, `EMAIL_SENDER_PASSWORD`; bạn phải map chúng sang key chuẩn ASP.NET Core nếu chạy bằng CLI
- Nếu `source .env` báo lỗi, hãy kiểm tra lại file `.env` theo cú pháp `KEY=VALUE`
