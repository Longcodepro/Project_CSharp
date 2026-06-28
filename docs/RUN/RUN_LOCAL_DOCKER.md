# Run Local Docker

## Muc tieu

Tai lieu nay dung cho local Docker stack cua TuneVault.

Ban se biet:

- image nao duoc build tu source local
- cach build tung image
- cach chay tung service rieng le
- cach chay full stack
- cach rebuild khi frontend/backend co cap nhat moi
- khi nao can reset database volume

## Stack local hien tai

Compose file:

```bash
docker-compose.local.yml
```

Service trong stack:

- `db`: SQL Server container
- `db-init`: container chay script tao DB + seed data, sau do thoat
- `backend`: ASP.NET Core API
- `frontend`: Nginx serve Vite build

## Port chuan

- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:5128`
- Swagger: `http://localhost:5128/swagger`
- SQL Server: `localhost:1433`

## Dieu kien truoc khi chay

Can co file `.env` o root project hoac export bien moi truong truoc khi chay compose.

Toi thieu:

```env
SQL_SERVER_PASSWORD=YOUR_SQL_PASSWORD
JWT_SECRET=YOUR_JWT_SECRET
ANTHROPIC_API_KEY=YOUR_ANTHROPIC_API_KEY
```

Ghi chu:

- `SQL_SERVER_PASSWORD` bat buoc cho `db`, `db-init`, `backend`
- `JWT_SECRET` bat buoc cho `backend`
- `ANTHROPIC_API_KEY` co the de rong neu flow local khong dung

## Image nao duoc build tu source local

### Frontend image

Build tu:

```bash
./frontend/Dockerfile
```

Compose service:

```bash
frontend
```

Image nay:

- dung `node:20-alpine` de build Vite
- chay bang `nginx:alpine`
- expose ra host `3000:80`

### Backend image

Build tu:

```bash
./backend/src/TuneVault.API/Dockerfile
```

Compose service:

```bash
backend
```

Image nay:

- build/publish bang `.NET 9 SDK`
- runtime bang `mcr.microsoft.com/dotnet/aspnet:9.0`
- expose ra host `5128:8080`

### Database image

`db` va `db-init` khong build tu source local.

Chung dung image co san:

```bash
mcr.microsoft.com/mssql/server:2022-latest
```

## Lenh co ban

Tat ca lenh ben duoi deu dung tu root project:

```bash
cd /duong-dan-toi-project
```

## 1. Chay full stack local

Lenh de dung nhat:

```bash
docker compose -f docker-compose.local.yml up --build
```

Lenh nay se:

- build `backend`
- build `frontend`
- start `db`
- chay `db-init`
- start `backend`
- start `frontend`

Neu muon chay detached:

```bash
docker compose -f docker-compose.local.yml up -d --build
```

## 2. Build image rieng tung phan

### Build chi frontend image

```bash
docker compose -f docker-compose.local.yml build frontend
```

### Build chi backend image

```bash
docker compose -f docker-compose.local.yml build backend
```

### Build ca frontend va backend

```bash
docker compose -f docker-compose.local.yml build frontend backend
```

Ghi chu:

- `db` va `db-init` khong can build vi dung image prebuilt

## 3. Chay tung service rieng

### Chay chi database

```bash
docker compose -f docker-compose.local.yml up -d db
```

### Chay script init + seed database

Lenh nay can `db` da healthy:

```bash
docker compose -f docker-compose.local.yml up db-init
```

Hoac neu `db` chua chay:

```bash
docker compose -f docker-compose.local.yml up -d db
docker compose -f docker-compose.local.yml up db-init
```

Ghi chu:

- `db-init` la one-shot container
- no chay xong se thoat
- no khong phai service luon song nhu `db`, `backend`, `frontend`

### Chay chi backend

```bash
docker compose -f docker-compose.local.yml up -d backend
```

Compose se tu dam bao dependency:

- `db`
- `db-init`

### Chay chi frontend

```bash
docker compose -f docker-compose.local.yml up -d frontend
```

Compose se tu keo theo:

- `backend`

### Chay backend + frontend nhung bo qua log attach

```bash
docker compose -f docker-compose.local.yml up -d backend frontend
```

## 4. Khi co cap nhat moi o mot image thi lam gi

### Truong hop A: chi sua frontend

Build lai frontend image va start lai frontend container:

```bash
docker compose -f docker-compose.local.yml build frontend
docker compose -f docker-compose.local.yml up -d frontend
```

Neu muon gon hon:

```bash
docker compose -f docker-compose.local.yml up -d --build frontend
```

Dung khi:

- sua React/Vite code
- sua `frontend/Dockerfile`
- sua `frontend/nginx.conf`

### Truong hop B: chi sua backend

Build lai backend image va start lai backend container:

```bash
docker compose -f docker-compose.local.yml build backend
docker compose -f docker-compose.local.yml up -d backend
```

Hoac:

```bash
docker compose -f docker-compose.local.yml up -d --build backend
```

Dung khi:

- sua code API
- sua `backend/src/TuneVault.API/Dockerfile`
- doi config runtime cua backend trong compose

### Truong hop C: sua ca frontend va backend

```bash
docker compose -f docker-compose.local.yml build frontend backend
docker compose -f docker-compose.local.yml up -d frontend backend
```

Hoac:

```bash
docker compose -f docker-compose.local.yml up -d --build frontend backend
```

### Truong hop D: sua SQL schema, seed, hoac init script

Chi rebuild `backend`/`frontend` la khong du.

Can chay lai `db-init`, va trong nhieu truong hop can reset volume DB:

```bash
docker compose -f docker-compose.local.yml down -v
docker compose -f docker-compose.local.yml up -d db
docker compose -f docker-compose.local.yml up db-init
docker compose -f docker-compose.local.yml up -d backend frontend
```

Dung khi:

- sua `database.sql`
- sua `seed.sql`
- sua `backend/src/TuneVault.Infrastructure/Database/init/*`

Neu khong reset volume thi DB cu co the van giu data/schema cu.

## 5. Khi nao can `up --build`

Nen dung:

```bash
docker compose -f docker-compose.local.yml up -d --build <service>
```

khi:

- ban vua sua source code
- ban vua sua Dockerfile
- ban khong chac image local da moi chua

Khong can `--build` khi:

- chi restart lai container
- image khong thay doi

## 6. Restart lai container ma khong rebuild

### Restart frontend

```bash
docker compose -f docker-compose.local.yml restart frontend
```

### Restart backend

```bash
docker compose -f docker-compose.local.yml restart backend
```

### Restart ca stack

```bash
docker compose -f docker-compose.local.yml restart
```

Lenh nay khong lay code moi neu image chua duoc rebuild.

## 7. Xem log

### Log frontend

```bash
docker compose -f docker-compose.local.yml logs -f frontend
```

### Log backend

```bash
docker compose -f docker-compose.local.yml logs -f backend
```

### Log database

```bash
docker compose -f docker-compose.local.yml logs -f db
```

### Log init database

```bash
docker compose -f docker-compose.local.yml logs db-init
```

## 8. Dung stack

### Dung container nhung giu volume DB

```bash
docker compose -f docker-compose.local.yml down
```

### Dung container va xoa volume DB

```bash
docker compose -f docker-compose.local.yml down -v
```

Can than:

- `down -v` se xoa volume `sql-data`
- lan chay sau se can init/seed lai database

## 9. Luong chay khuyen nghi

### Lan dau chay local Docker

```bash
docker compose -f docker-compose.local.yml up -d --build
```

Sau do mo:

- `http://localhost:3000`
- `http://localhost:5128/swagger`

### Sau khi chi sua frontend

```bash
docker compose -f docker-compose.local.yml up -d --build frontend
```

### Sau khi chi sua backend

```bash
docker compose -f docker-compose.local.yml up -d --build backend
```

### Sau khi sua SQL init/seed

```bash
docker compose -f docker-compose.local.yml down -v
docker compose -f docker-compose.local.yml up -d --build
```

## 10. Kiem tra nhanh stack dang chay

```bash
docker compose -f docker-compose.local.yml ps
```

Ban nen thay:

- `db` dang `healthy`
- `backend` dang `running`
- `frontend` dang `running`
- `db-init` da `exited (0)` sau khi chay xong
