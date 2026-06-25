# Cách chạy TuneVault local trên Windows

Tài liệu này dành cho môi trường Windows. Nếu dùng Linux/macOS thì đọc `RUN_LOCAL.md`.

## 1. Chạy Backend API

Mở PowerShell tại **root project**.

### Cách 1: PowerShell

Nạp biến môi trường cho phiên hiện tại:

```powershell
$env:SQL_SERVER_PASSWORD="mat_khau_sa"
$env:JWT_SECRET="chuoi_bi_mat_bi_mat_du_dai"
$env:EMAIL_SENDER_EMAIL="your_sender_email@example.com"
$env:EMAIL_SENDER_PASSWORD="your_sender_password"
$env:EMAIL_SMTP_HOST="smtp.example.com"
$env:EMAIL_SMTP_PORT="587"

$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DatabaseOptions__ConnectionString="Server=localhost,1433;Database=TuneVault;User Id=sa;Password=$env:SQL_SERVER_PASSWORD;TrustServerCertificate=True;"
$env:JwtSettings__SecretKey=$env:JWT_SECRET
$env:JwtSettings__Issuer="TuneVault_Backend_API"
$env:JwtSettings__Audience="TuneVault_Client_Application"
$env:EmailSettings__SenderEmail=$env:EMAIL_SENDER_EMAIL
$env:EmailSettings__SenderPassword=$env:EMAIL_SENDER_PASSWORD
```

Chạy backend:

```powershell
dotnet run --project src/TuneVault.API/TuneVault.API.csproj --urls http://127.0.0.1:5128
```

Kiểm tra:

```text
http://127.0.0.1:5128/swagger
http://127.0.0.1:5128/health
```

### Cách 2: Command Prompt `cmd`

Nạp biến môi trường cho cửa sổ hiện tại:

```bat
set SQL_SERVER_PASSWORD=mat_khau_sa
set JWT_SECRET=chuoi_bi_mat_bi_mat_du_dai
set EMAIL_SENDER_EMAIL=your_sender_email@example.com
set EMAIL_SENDER_PASSWORD=your_sender_password
set EMAIL_SMTP_HOST=smtp.example.com
set EMAIL_SMTP_PORT=587

set ASPNETCORE_ENVIRONMENT=Development
set DatabaseOptions__ConnectionString=Server=localhost,1433;Database=TuneVault;User Id=sa;Password=%SQL_SERVER_PASSWORD%;TrustServerCertificate=True;
set JwtSettings__SecretKey=%JWT_SECRET%
set JwtSettings__Issuer=TuneVault_Backend_API
set JwtSettings__Audience=TuneVault_Client_Application
set EmailSettings__SenderEmail=%EMAIL_SENDER_EMAIL%
set EmailSettings__SenderPassword=%EMAIL_SENDER_PASSWORD%
```

Chạy backend:

```bat
dotnet run --project src/TuneVault.API/TuneVault.API.csproj --urls http://127.0.0.1:5128
```

## 2. Chạy Frontend

Mở terminal mới tại root project, sau đó vào thư mục `frontend`:

```powershell
cd frontend
```

Cài dependencies nếu cần:

```powershell
npm install
```

Chạy frontend:

```powershell
npm run dev -- --host 127.0.0.1 --port 5174
```

Mở web:

```text
http://127.0.0.1:5174
```

## 3. Lưu ý thường gặp

- Biến môi trường đặt bằng `$env:...` chỉ có hiệu lực trong cửa sổ PowerShell hiện tại.
- Biến đặt bằng `set` trong `cmd` cũng chỉ có hiệu lực trong cửa sổ đó.
- Không nên dùng `setx` cho secret local nếu không cần, vì nó lưu lâu hơn.
- Nếu backend báo lỗi connection string, kiểm tra lại:
  - `SQL_SERVER_PASSWORD`
  - `JWT_SECRET`
  - `EMAIL_SENDER_EMAIL`
  - `EMAIL_SENDER_PASSWORD`
- Nếu frontend lỗi CORS, kiểm tra backend đã cho phép origin:
  - `http://127.0.0.1:5174`
  - `http://localhost:5174`

## 4. Gợi ý nhanh

- Dev local trên Windows: dùng PowerShell là tiện nhất.
- Nếu team đang chạy script trong `cmd`, dùng phần `cmd` ở trên.
- Nếu muốn cấu hình lâu dài, nên tạo file `.env` riêng cho local thay vì hardcode trong script.
