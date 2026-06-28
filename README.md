# TuneVault

TuneVault la ung dung nghe nhac va quan ly media gom backend ASP.NET Core, frontend React/Vite va SQL Server.

## Cau truc

- `backend/src`: source backend .NET.
- `frontend`: source frontend React/Vite.
- `database.sql`: schema snapshot.
- `seed.sql`: du lieu mau.
- `docker-compose.yml`: chay full stack bang Docker.
- `docker-compose.local.yml`: cau hinh Docker local.
- `docker-compose.pro.yml`: cau hinh Docker production.

## Cau hinh moi truong

Tao file `.env` o root project theo mau:

```env
SQL_SERVER_PASSWORD=YOUR_SQL_PASSWORD
JWT_SECRET=YOUR_JWT_SECRET
ANTHROPIC_API_KEY=YOUR_ANTHROPIC_API_KEY
```

Khong commit file `.env` hoac file cau hinh co password that.

## Chay bang Docker

```bash
docker compose -f docker-compose.local.yml up -d --build
```

Mac dinh:

- Frontend: `http://localhost:3000`
- Backend: `http://localhost:5128`
- Swagger: `http://localhost:5128/swagger`
- SQL Server: `localhost:1433`

Reset database local:

```bash
docker compose -f docker-compose.local.yml down -v
docker compose -f docker-compose.local.yml up -d --build
```

## Chay rieng frontend

```bash
cd frontend
npm install
npm run dev
```

## Chay rieng backend

```bash
dotnet restore backend/src/TuneVault.sln
dotnet build backend/src/TuneVault.sln
dotnet run --project backend/src/TuneVault.API/TuneVault.API.csproj --urls http://localhost:5128
```

## Tai khoan seed

Quy uoc password: `idDisplay + 123`.

| Vai tro | IdDisplay | Password |
| --- | --- | --- |
| Listener | listener_one | listener_one123 |
| Listener | listener_two | listener_two123 |
| Artist | sontungmtp | sontungmtp123 |
| Artist | justinbieber | justinbieber123 |
| Artist | shakira | shakira123 |
| Artist | mono | mono123 |
| Artist | soobin | soobin123 |
