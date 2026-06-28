# Local CLI

## Muc tieu

Chay local tren may dev bang CLI, tach rieng database, backend va frontend.

## Port chuan

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:5128`
- Swagger: `http://localhost:5128/swagger`
- Database: `localhost:1433`

## 1. Khoi tao database va seed data

```bash
docker compose -f docker-compose.local.yml up -d db-init
```

Lenh nay se:

- tao `TuneVaultDb` neu chua co
- chay `database.sql`
- chay `seed.sql`

## 2. Chay backend

```bash
source .env
dotnet restore backend/src/TuneVault.sln
dotnet build backend/src/TuneVault.sln
ASPNETCORE_ENVIRONMENT=Development \
DatabaseOptions__ConnectionString="Server=localhost,1433;Database=TuneVaultDb;User Id=sa;Password=${SQL_SERVER_PASSWORD};TrustServerCertificate=True;" \
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=TuneVaultDb;User Id=sa;Password=${SQL_SERVER_PASSWORD};TrustServerCertificate=True;" \
JwtSettings__SecretKey="${JWT_SECRET}" \
JwtSettings__Issuer="TuneVault_Backend_API" \
JwtSettings__Audience="TuneVault_Client_Application" \
dotnet run --project backend/src/TuneVault.API/TuneVault.API.csproj --urls http://localhost:5128
```

## 3. Chay frontend

```bash
cd frontend
npm install
npm run dev
```

## Ghi chu

- Frontend Vite da proxy `/api`, `/uploads`, `/hubs` ve backend `5128`.
- `MediaService.tsx` mac dinh dung same-origin `/api`.
- Neu backend khong ket noi duoc DB, kiem tra `SQL_SERVER_PASSWORD` va database name `TuneVaultDb`.
