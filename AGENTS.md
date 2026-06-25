# AGENTS.md — TuneVault AI Coding Rules

> File này là luật bắt buộc cho mọi AI agent khi làm việc trên project TuneVault.
> Trước khi sửa code, agent phải đọc file này và các file markdown liên quan được liệt kê bên dưới.
> Không được code ngay khi chưa phân tích yêu cầu, chưa kiểm tra file liên quan, hoặc chưa lập kế hoạch.

---

## 1. Mục tiêu của file này

File này giúp AI:

* Hiểu project TuneVault đang ở giai đoạn hoàn thiện cuối.
* Biết phải đọc file markdown nào trước khi code.
* Không sửa lung tung những file không liên quan.
* Tuân thủ đúng quy trình khi sửa frontend, nối endpoint, hoặc thêm endpoint backend.
* Ghi lại thay đổi sau khi làm xong để developer dễ kiểm soát.

---

## 2. File markdown bắt buộc cần biết

Project có nhiều file hướng dẫn. Agent phải đọc đúng file theo ngữ cảnh, không đọc lan man và không bỏ qua file quan trọng.

### 2.1 File ở root project

| File                | Khi nào phải đọc?                                    | Mục đích                                                             |
| ------------------- | ---------------------------------------------------- | -------------------------------------------------------------------- |
| `README.md`         | Khi cần hiểu tổng quan project hoặc chuẩn bị nộp bài | Mô tả project, công nghệ, cách chạy tổng quan, tài khoản seed nếu có |
| `AGENTS.md`         | Luôn đọc đầu tiên                                    | Luật làm việc bắt buộc cho AI                                        |
| `DOCKER_GUIDE.md`    | Khi cần build/chạy project bằng Docker              | Hướng dẫn `.env`, `docker compose`, build, up, down, clean          |
| `API_CONTRACT.md`   | Khi nối frontend với backend hoặc sửa endpoint       | Danh sách endpoint, request/response, DTO, route frontend đang dùng  |
| `CURRENT_STATUS.md` | Luôn đọc trước khi làm task mới                      | Trạng thái hiện tại: đã xong gì, đang lỗi gì, ưu tiên tiếp theo      |

### 2.2 File trong `docs/archive/`

| File                                                | Khi nào đọc?                                        | Mục đích                                                               |
| --------------------------------------------------- | --------------------------------------------------- | ---------------------------------------------------------------------- |
| `docs/archive/TuneVault_BaiTapLon.pdf`              | Khi cần đối chiếu yêu cầu gốc của đồ án             | Yêu cầu chính thức: 10 chức năng, frontend, backend, pipeline, nộp bài |
| `docs/archive/DATABASE_SCHEMA.md`                   | Khi sửa database, repository, SQL, DTO liên quan DB | Cấu trúc bảng, field, quan hệ dữ liệu                                  |
| `docs/archive/ENDPOINT_PERMISSION_AUDIT.md`         | Khi sửa phân quyền hoặc endpoint cần `[Authorize]`  | Kiểm tra endpoint nào cần đăng nhập, quyền owner, quyền user           |
| `docs/archive/DESIGN-spotify.md`                    | Khi sửa UI/layout frontend                          | Quy chuẩn giao diện Spotify-like                                       |

### 2.3 Folder `PLANS/`

Folder `PLANS/` dùng để lưu kế hoạch theo từng task.

Agent nên tạo file plan mới khi task có nhiều bước hoặc có rủi ro ảnh hưởng nhiều file.

Ví dụ:

```txt
PLANS/
  001-fix-login-frontend.md
  002-connect-playlist-api.md
  003-add-notification-endpoint.md
```

---

## 3. Thứ tự đọc file trước khi code

### 3.1 Mọi task đều phải đọc

Trước khi làm bất kỳ task nào, đọc theo thứ tự:

```txt
1. AGENTS.md
2. CURRENT_STATUS.md
3. DOCKER_GUIDE.md nếu task liên quan Docker/build/chạy project
```

### 3.2 Nếu task là sửa frontend

Đọc thêm:

```txt
4. API_CONTRACT.md
5. docs/archive/DESIGN-spotify.md
```

### 3.3 Nếu task là nối endpoint vào frontend

Đọc thêm:

```txt
4. API_CONTRACT.md
5. docs/archive/ENDPOINT_PERMISSION_AUDIT.md
```

Sau đó phải kiểm tra code backend xem endpoint đó đã tồn tại thật chưa.

### 3.4 Nếu task là thêm/sửa endpoint backend

Đọc thêm:

```txt
4. API_CONTRACT.md
5. docs/archive/DATABASE_SCHEMA.md
6. docs/archive/ENDPOINT_PERMISSION_AUDIT.md
```

### 3.5 Nếu task liên quan yêu cầu đồ án

Đọc thêm:

```txt
docs/archive/TuneVault_BaiTapLon.pdf
```

---

## 4. Quy trình bắt buộc trước khi code

Không được code ngay.

Mọi task phải đi qua 4 bước:

```txt
Bước 1: Hiểu yêu cầu
Bước 2: Kiểm tra file liên quan
Bước 3: Lập kế hoạch sửa
Bước 4: Chỉ code sau khi đã có kế hoạch
```

Kế hoạch phải ghi rõ:

* Mục tiêu task.
* Những file dự kiến sửa.
* Những file chỉ đọc, không sửa.
* Cách kiểm tra sau khi sửa.
* Rủi ro có thể gặp.
* Có cần hỏi lại developer không.

Nếu thiếu dữ liệu, thiếu endpoint, thiếu DTO, hoặc không chắc business rule, phải hỏi lại. Không được tự đoán dữ liệu.

---

## 5. Quy trình sửa lỗi frontend

Dùng khi developer yêu cầu sửa giao diện, layout, state, service, route, component hoặc bug frontend.

### 5.1 Quy trình

```txt
1. Đọc AGENTS.md
2. Đọc CURRENT_STATUS.md
3. Đọc DOCKER_GUIDE.md nếu task cần build/chạy Docker hoặc local
4. Đọc API_CONTRACT.md nếu frontend có gọi API
5. Đọc DESIGN-spotify.md nếu liên quan UI
6. Phân tích lỗi/yêu cầu
7. Lập kế hoạch sửa
8. Sửa đúng file frontend liên quan
9. Chạy kiểm tra
10. Báo cáo kết quả
```

### 5.2 Lệnh kiểm tra frontend bắt buộc

Chạy trong thư mục frontend hoặc đúng thư mục được `DOCKER_GUIDE.md` hướng dẫn:

```bash
npm install
npm run dev
```

Nếu có lệnh build/test riêng trong project thì có thể chạy thêm, nhưng không thay thế 2 lệnh trên nếu developer yêu cầu.

### 5.3 Luật khi sửa frontend

* Không sửa backend nếu task chỉ là frontend.
* Không đổi toàn bộ layout nếu chỉ sửa một lỗi nhỏ.
* Không hardcode dữ liệu nếu API đã có.
* Không tự tạo endpoint giả nếu chưa được yêu cầu.
* Component mới phải rõ tên, dễ hiểu.
* Service API phải dùng endpoint trong `API_CONTRACT.md` hoặc endpoint thực tế trong backend.
* Nếu endpoint thiếu, chuyển sang quy trình nối endpoint hoặc thêm endpoint backend.

---

## 6. Quy trình nối endpoint vào frontend

Dùng khi developer yêu cầu một chức năng frontend gọi backend thật.

### 6.1 Quy trình

```txt
1. Đọc AGENTS.md
2. Đọc CURRENT_STATUS.md
3. Đọc DOCKER_GUIDE.md nếu task cần build/chạy Docker hoặc local
4. Đọc API_CONTRACT.md
5. Kiểm tra code backend xem endpoint có tồn tại thật không
6. Kiểm tra DTO request/response thật
7. Nếu endpoint đã có: dùng endpoint đó
8. Nếu endpoint chưa có: chuyển sang quy trình thêm endpoint backend
9. Sửa service/frontend state/component liên quan
10. Run thử
11. Báo cáo kết quả
```

### 6.2 Luật kiểm tra endpoint

Trước khi dùng endpoint, phải xác nhận:

* HTTP method đúng.
* Route đúng.
* Có cần JWT không.
* Request body đúng DTO.
* Response trả về shape nào.
* Frontend có xử lý loading/error không.
* Trường hợp 401/403/404/500 được xử lý hợp lý.

### 6.3 Không được làm

* Không tự đoán route.
* Không tự đổi route backend chỉ vì frontend đang gọi sai.
* Không tạo service gọi API chưa tồn tại.
* Không bỏ qua auth token nếu endpoint yêu cầu đăng nhập.
* Không parse response bừa nếu chưa kiểm tra DTO thật.

---

## 7. Quy trình thêm endpoint backend

Dùng khi chức năng cần endpoint nhưng backend chưa có.

### 7.1 Thứ tự làm backend bắt buộc

Backend phải đi theo hướng Clean Architecture.

Thứ tự ưu tiên:

```txt
1. Domain / Interface nếu cần
2. Application Feature
3. DTO / Command / Query / Handler
4. Repository interface nếu chưa có
5. Infrastructure repository implementation
6. DI registration
7. API Controller
8. Cập nhật API_CONTRACT.md
9. Run kiểm tra
10. Ghi lại trong commit message hoặc báo cáo cuối
```

Nếu task đơn giản và project hiện tại đã có pattern khác, được giữ style hiện tại của feature đó, nhưng không được phá dependency rule.

### 7.2 Repository

Repository chỉ xử lý truy vấn dữ liệu.

Không đặt business logic trong repository.

Code mới nên dùng:

```csharp
IDbConnectionFactory
```

Không nhân rộng `DapperContext` nếu không bắt buộc.

SQL phải dùng parameter:

```csharp
WHERE Id = @Id
```

Không nối chuỗi SQL bằng interpolation với input người dùng.

### 7.3 Feature / Handler

Business logic nằm trong Handler.

Mỗi Command/Query phải có Handler tương ứng.

Command/Query nên implement:

```csharp
IRequest<TResponse>
```

Handler có nhiệm vụ:

```txt
Validate nghiệp vụ
Kiểm tra quyền nếu cần
Gọi repository
Map sang DTO
Trả response
```

### 7.4 Controller

Controller chỉ nên:

```txt
Nhận request
Gọi _mediator.Send(...)
Trả response
```

Không viết truy vấn DB trong controller.

Không viết business logic dài trong controller.

### 7.5 Comment và summary

Method mới hoặc class mới phải có summary ngắn gọn.

Trong method có nhiều bước xử lý, thêm comment theo từng bước.

Ví dụ:

```csharp
/// <summary>
/// Handles creating a new playlist for the authenticated user.
/// </summary>
public async Task<PlaylistDto> Handle(CreatePlaylistCommand request, CancellationToken ct)
{
    // Step 1: Validate owner/user existence.
    // Step 2: Create playlist entity.
    // Step 3: Persist playlist.
    // Step 4: Map result to response DTO.
}
```

Không comment lan man những dòng quá hiển nhiên.

---

## 8. Quy tắc chạy thử và giới hạn số lần run

### 8.1 Số lần run tối đa

Agent chỉ được run tối đa 2 lần cho cùng một hướng sửa.

Nếu sau 2 lần vẫn lỗi:

```txt
1. Dừng code
2. Phân tích lại nguyên nhân
3. Ghi rõ lỗi nằm ở đâu
4. Lập kế hoạch mới
5. Chỉ tiếp tục khi đã có hướng sửa mới
```

Không được sửa mò liên tục.

### 8.2 Với frontend

Ưu tiên chạy:

```bash
npm install
npm run dev
```

Nếu có lỗi TypeScript hoặc build, đọc lỗi rồi sửa đúng nguyên nhân.

### 8.3 Với backend

Tùy cấu trúc project trong `DOCKER_GUIDE.md`, dùng lệnh phù hợp như:

```bash
dotnet restore
dotnet build
dotnet run
```

Không được tự đổi connection string thật.

Không commit secret, password, JWT key thật.

---

## 9. Quy tắc cập nhật CURRENT_STATUS.md

Sau task lớn, nếu trạng thái project thay đổi, cập nhật `CURRENT_STATUS.md`.

Nên cập nhật khi:

* Hoàn thành một chức năng.
* Fix xong một lỗi quan trọng.
* Thêm endpoint mới.
* Nối xong frontend với endpoint.
* Phát hiện bug lớn chưa sửa.
* Có việc cần developer quyết định.

Không cần cập nhật nếu chỉ sửa typo nhỏ.

---

## 10. Quy tắc cập nhật API_CONTRACT.md

Phải cập nhật `API_CONTRACT.md` khi:

* Thêm endpoint mới.
* Đổi route endpoint.
* Đổi request DTO.
* Đổi response DTO.
* Đổi yêu cầu auth/role.
* Frontend bắt đầu dùng endpoint mới.

Mỗi endpoint nên ghi:

````md
### METHOD /api/route

Auth: Required / Not required

Request:
```json
{}
````

Response:

```json
{}
```

Ghi chú:

* Mô tả ngắn chức năng.

````

---

## 11. Quy tắc Clean Architecture

Project theo Clean Architecture:

```txt
Domain → Application → Infrastructure → API
````

Quy tắc dependency:

* `Domain` không phụ thuộc layer khác.
* `Application` chỉ phụ thuộc `Domain`.
* `Infrastructure` phụ thuộc `Application` và `Domain`.
* `API` gọi `Application`, không gọi trực tiếp database trong controller.

Không được import ngược layer.

Không được đưa logic nghiệp vụ vào controller.

Không được để frontend quyết định nghiệp vụ bảo mật thay backend.

---

## 12. Quy tắc Dapper và SQL

Khi viết repository bằng Dapper:

* Luôn mở connection bằng `using var`.
* Luôn dùng parameterized query.
* Không nối chuỗi SQL với input người dùng.
* Nếu có nhiều thao tác ghi liên quan nhau, cân nhắc transaction.
* Không trả entity thô ra controller nếu đã có DTO.
* Không hardcode connection string.

Ví dụ đúng:

```csharp
using var conn = _db.CreateConnection();

var sql = """
    SELECT *
    FROM MediaItems
    WHERE Id = @Id
""";

return await conn.QuerySingleOrDefaultAsync<MediaItem>(sql, new { Id = id });
```

---

## 13. Quy tắc JWT/Auth

Project đang có một số bất nhất về JWT/Auth, agent phải cẩn thận.

Ưu tiên dùng:

```csharp
IJwtTokenGenerator
```

Không dùng `ITokenService` nếu đó là service stub hoặc không còn được dùng.

Khi sửa auth, phải kiểm tra:

* JWT config trong `Program.cs`.
* SecretKey/Issuer/Audience có thống nhất không.
* `UseAuthentication()` có bật không.
* `UseAuthorization()` có bật không.
* Endpoint nhạy cảm có `[Authorize]` không.
* Frontend có gửi Bearer token không.

Không tự đổi toàn bộ auth flow nếu task không yêu cầu.

---

## 14. Quy tắc response format

Response nên thống nhất theo dạng:

```json
{
  "success": true,
  "data": {},
  "message": null
}
```

Hoặc lỗi:

```json
{
  "success": false,
  "data": null,
  "message": "Mô tả lỗi"
}
```

Nếu project hiện tại chưa có `ApiResponse<T>`, chỉ tạo/sửa khi task liên quan response format hoặc endpoint mới cần dùng.

Không đổi toàn bộ response cũ nếu có thể làm vỡ frontend hiện tại, trừ khi developer yêu cầu chuẩn hóa.

---

## 15. Quy tắc xử lý thiếu dữ liệu

Nếu thiếu dữ liệu, thiếu field, thiếu endpoint, thiếu rule nghiệp vụ hoặc không chắc ý developer, phải hỏi lại.

Không được:

* Tự bịa dữ liệu.
* Tự đổi schema.
* Tự đổi route.
* Tự thêm role.
* Tự thêm field DB.
* Tự đổi business rule.
* Tự xóa file nghi là dư thừa.

Nếu phát hiện file thừa hoặc code chết, chỉ báo cáo và đề xuất xóa. Không tự xóa nếu developer chưa đồng ý.

---

## 16. Quy tắc không sửa file không liên quan

Trước khi sửa, agent phải liệt kê file dự kiến sửa.

Trong quá trình làm:

* Chỉ sửa file liên quan trực tiếp.
* Không format toàn bộ project.
* Không đổi tên hàng loạt.
* Không refactor lớn nếu task chỉ là fix nhỏ.
* Không sửa cả frontend và backend nếu task chỉ yêu cầu một phía, trừ khi có lý do rõ ràng và phải báo trước.

---

## 17. Những lỗi/bẫy đã biết trong project

Agent phải kiểm tra lại trong code hiện tại trước khi kết luận, nhưng cần đặc biệt chú ý các điểm sau:

1. Có thể tồn tại nhiều convention DTO song song.
2. Một số interface/service auth có thể bị trùng hoặc là stub.
3. JWT generator và JWT validation có thể chưa thống nhất config.
4. Có endpoint yêu cầu auth nhưng frontend chưa gửi token.
5. Có endpoint trả 401 là hợp lệ nếu user nhập sai login.
6. Console browser có thể hiện lỗi extension/Chrome built-in AI, không thuộc app.
7. Một số markdown trong `docs/archive/` chỉ là tài liệu tham khảo, không phải file cần sửa.
8. Không phải mọi file hiện có đều đúng chuẩn; phải kiểm tra reference thật trước khi dùng.

---

## 18. Checklist trước khi báo hoàn thành

Trước khi báo thành công, agent phải tự kiểm tra:

* [ ] Đã đọc `AGENTS.md`.
* [ ] Đã đọc `CURRENT_STATUS.md`.
* [ ] Đã đọc file markdown liên quan.
* [ ] Đã lập kế hoạch trước khi code.
* [ ] Chỉ sửa file liên quan.
* [ ] Không tự đoán dữ liệu thiếu.
* [ ] Không hardcode secret.
* [ ] Không tạo endpoint giả.
* [ ] Không để SQL injection.
* [ ] Đã chạy lệnh kiểm tra phù hợp.
* [ ] Nếu run lỗi quá 2 lần, đã dừng và phân tích lại.
* [ ] Đã cập nhật `API_CONTRACT.md` nếu có đổi/thêm endpoint.
* [ ] Đã cập nhật `CURRENT_STATUS.md` nếu trạng thái project thay đổi.
* [ ] Báo cáo cuối có nêu rõ sửa gì, file nào, test ra sao.

---

## 19. Format báo cáo cuối cho developer

Sau khi làm xong, báo cáo ngắn gọn theo mẫu:

```md
## Kết quả

Đã hoàn thành: <mô tả ngắn>

## File đã sửa

- `file1`
- `file2`

## Kiểm tra

- `npm install`: pass/fail/not run
- `npm run dev`: pass/fail/not run
- `dotnet build`: pass/fail/not run

## Ghi chú

- Vấn đề còn lại nếu có.
- Điều cần developer xác nhận nếu có.
```

Không báo “thành công” nếu chưa chạy được hoặc chưa kiểm tra theo yêu cầu.
