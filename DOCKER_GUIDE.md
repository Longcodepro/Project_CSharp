# DOCKER_GUIDE — TuneVault

Hướng dẫn chạy project hiện tại bằng Docker.

## 1. Yêu cầu trước khi chạy

Tạo file `.env` ở cùng cấp với `docker-compose.yml`:

```env
SQL_SERVER_PASSWORD=mat_khau_cua_ban
JWT_SECRET=chuoi_bi_mat_dai_va_kho_doan
ANTHROPIC_API_KEY=api_key_cua_ban
```

Không commit file `.env`.

## 2. Build và chạy

Chạy ở root `Project_CSharp/`:

```bash
docker compose up --build
```

Nếu muốn chạy nền:

```bash
docker compose up -d --build
```

## 3. Dừng hệ thống

```bash
docker compose down
```

Xóa luôn dữ liệu database volume:

```bash
docker compose down -v
```

## 4. Port mặc định

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:5000`
- SQL Server: `localhost:1433`

## 5. Cấu trúc Docker hiện tại

- Backend build từ `backend/src`
- Frontend build từ `frontend`
- SQL Server dùng image `mcr.microsoft.com/mssql/server:2022-latest`
- Backend mount uploads từ `backend/src/TuneVault.API/wwwroot/uploads`

## 6. Lưu ý

- Nếu thiếu `SQL_SERVER_PASSWORD`, compose sẽ báo lỗi ngay.
- Backend đang target `.NET 9`.
- Frontend phải build qua `vite build` trước khi vào nginx.

  - Frontend: http://localhost:3000
  - Backend: http://localhost:5000
  - SQL Server: localhost:1433
