# Docker Server

## Muc tieu

Chay production stack voi frontend public tren port 80, backend va database chi noi bo.

## Compose

```bash
docker compose -f docker-compose.pro.yml up --build -d
```

## Current URLs

- Frontend public: `http://localhost`
- Backend: internal only
- Database: internal only

## Proxy

- `/api` -> backend container
- `/uploads` -> backend container
- `/hubs` -> backend container, co ho tro WebSocket
- `/health` -> backend container

## Ghi chu

- Frontend nginx da serve SPA fallback ve `index.html`.
- Production backend chay voi `ASPNETCORE_ENVIRONMENT=Production`.
