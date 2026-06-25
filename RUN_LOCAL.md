# Cách chạy TuneVault local

## 1. Chạy Backend API

Mở terminal tại **root project** — thư mục có `src/`, `client/`, `.env`.

Nạp biến môi trường:

```bash
set -a
source .env
set +a

# Email settings for sending OTP
export EMAIL_SENDER_EMAIL="${EMAIL_SENDER_EMAIL}"
export EMAIL_SENDER_PASSWORD="${EMAIL_SENDER_PASSWORD}"
export EMAIL_SMTP_HOST="${EMAIL_SMTP_HOST}"
export EMAIL_SMTP_PORT="${EMAIL_SMTP_PORT}"
>>>>>>> Stashed changes
```

Chạy backend:

```bash
`export ASPNETCORE_ENVIRONMENT="Development"
export DatabaseOptions__ConnectionString="Server=localhost,1433;Database=TuneVault;User Id=sa;Password=${SQL_SERVER_PASSWORD};TrustServerCertificate=True;"
export JwtSettings__SecretKey="${JWT_SECRET}"
export JwtSettings__Issuer="TuneVault_Backend_API"
export JwtSettings__Audience="TuneVault_Client_Application"
export EmailSettings__SenderEmail="${EMAIL_SENDER_EMAIL}"
export EmailSettings__SenderPassword="${EMAIL_SENDER_PASSWORD}"

dotnet run --project src/TuneVault.API/TuneVault.API.csproj --urls http://127.0.0.1:5128`
```

Kiểm tra backend:

```text
http://127.0.0.1:5128/swagger
http://127.0.0.1:5128/health
```

> Không tắt terminal này khi đang dùng web.

---

## 2. Chạy Frontend Web

Mở **terminal mới** tại root project, sau đó vào thư mục `client`:

```bash
cd client
```

Nếu lần đầu chạy project:

```bash
npm install
```

Chạy frontend:

```bash
npm run dev -- --host 127.0.0.1 --port 5174
```

Mở web:

```text
http://127.0.0.1:5174
```

---

## 3. Lưu ý lỗi thường gặp

Nếu frontend báo lỗi CORS, kiểm tra backend đã cho phép origin:

```text
http://127.0.0.1:5174
http://localhost:5174
```

Nếu backend báo lỗi:

```text
The ConnectionString property has not been initialized
```

hãy kiểm tra `.env` có đủ biến:

```env
SQL_SERVER_PASSWORD=mat_khau_sa
JWT_SECRET=chuoi_bi_mat_du_dai
export EMAIL_SENDER_EMAIL="your_sender_email@example.com"
export EMAIL_SENDER_PASSWORD="your_sender_password"
export EMAIL_SMTP_HOST="smtp.example.com"
export EMAIL_SMTP_PORT="587"
>>>>>>> Stashed changes
```

Kiểm tra terminal đã đọc biến chưa:

```bash
echo "$SQL_SERVER_PASSWORD"
echo "$JWT_SECRET"
echo "$DatabaseOptions__ConnectionString"
echo "$EMAIL_SENDER_EMAIL"
echo "$EMAIL_SENDER_PASSWORD"
echo "$EMAIL_SMTP_HOST"
echo "$EMAIL_SMTP_PORT"
>>>>>>> Stashed changes
```
