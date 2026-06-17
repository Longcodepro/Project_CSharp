# LINUX_MINT_ENVIRONMENT_PLAN — Môi trường phát triển

> **Mục đích:** Nhắc AI Agent dùng lệnh phù hợp với Linux Mint, không sinh lệnh Windows/PowerShell.

---

## 1. Hệ điều hành chính

```text
OS: Linux Mint
Shell: bash
Database: SQL Server chạy bằng Docker
Backend: ASP.NET Core / .NET 9
Frontend: React/Vite
```

---

## 2. Không được dùng

AI Agent không được dùng các lệnh/script mặc định cho Windows:

```text
❌ PowerShell
❌ .bat files
❌ backslash path kiểu Windows
❌ Visual Studio-only workflow
❌ LocalDB mặc định Windows nếu người dùng chưa xác nhận
```

---

## 3. Backend commands

Chạy từ root project, cùng cấp với `TuneVault.sln`.

```bash
dotnet restore
dotnet build TuneVault.sln
```

Nếu cần chạy API:

```bash
cd src/TuneVault.API
dotnet run
```

Nếu cần watch:

```bash
cd src/TuneVault.API
dotnet watch run
```

---

## 4. Frontend commands

Frontend hiện nằm trong `client/` theo codebase hiện tại.

```bash
cd client
npm install
npm run dev
npm run build
```

Nếu thư mục frontend đổi, kiểm tra `package.json` trước.

---

## 5. SQL Server Docker

Database chạy bằng Docker. AI Agent có thể hướng dẫn kiểm tra container, nhưng không tự đổi schema nếu chưa được yêu cầu.

Lệnh kiểm tra gợi ý:

```bash
docker ps
```

Nếu cần xem log:

```bash
docker logs <container-name-or-id>
```

Nếu cần kết nối DB, phải hỏi người dùng connection string hoặc tên container nếu chưa rõ.

---

## 6. Path convention

Dùng path Linux:

```text
src/TuneVault.API/Program.cs
client/package.json
```

Không dùng:

```text
src\TuneVault.API\Program.cs
```

---

## 7. File nhạy cảm

Không tự in hoặc commit nội dung các file chứa secret:

- `appsettings.Development.json`
- `.env`
- `.env.local`
- connection string thật
- JWT secret
- API key

---

## 8. Khi build lỗi

Làm đúng Error Resolution Protocol trong `AGENTS.md`:

```text
Build lần 1 → sửa lỗi → Build lần 2 → nếu vẫn lỗi thì dừng và báo cáo.
```

Không tiếp tục implement thêm khi build vẫn lỗi.
