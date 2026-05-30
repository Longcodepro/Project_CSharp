# Project_CSharp
Đồ án môn CSharp về đề tài Media Streaming Web Application.

## Cấu trúc hiện tại

- `src/TuneVault.Domain`: entity, enum, exception, interface cốt lõi
- `src/TuneVault.Application`: tầng use case sau này
- `src/TuneVault.Infrastructure`: tầng hạ tầng sau này
- `src/TuneVault.API`: API host và Dockerfile backend
- `client`: frontend skeleton và Dockerfile frontend
- `docker-compose.yml`: dựng toàn bộ stack local

## Chạy Docker

```bash
docker compose up --build
```

- Frontend: `http://localhost:3000`
- Backend health: `http://localhost:5000/health`

## Ghi chú

- Skeleton này chỉ thêm phần tối thiểu để bắt đầu chia việc và phát triển tiếp.
- Các project vẫn giữ kiến trúc 4 lớp, chưa nhồi logic nghiệp vụ vào API.
