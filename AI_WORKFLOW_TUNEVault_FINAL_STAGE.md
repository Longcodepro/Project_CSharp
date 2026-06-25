# AI Workflow Chuẩn Hóa Giai Đoạn Cuối Project TuneVault

> File này dùng để hướng dẫn AI tuân theo một quy trình làm việc rõ ràng khi sửa frontend, nối API, hoặc thêm endpoint backend trong project TuneVault.  
> Mục tiêu chính: **không sửa lan man, không đoán dữ liệu, có kế hoạch trước khi code, kiểm tra sau khi sửa, và ghi lại lịch sử thay đổi vào markdown**.

---

## 1. Nguyên tắc chung bắt buộc

Trước khi bắt đầu bất kỳ công việc nào, AI phải tuân thủ các nguyên tắc sau:

1. **Đọc kỹ yêu cầu của người dùng trước khi code.**
2. **Không bắt đầu sửa code ngay.** Luôn phải lập kế hoạch trước.
3. **Không chỉnh sửa file không liên quan.**
4. **Không tự ý xóa file, class, method, endpoint nếu người dùng không yêu cầu.**
5. **Không tự đoán dữ liệu quan trọng.** Nếu thiếu dữ liệu cần thiết, phải hỏi lại người dùng.
6. **Không hard-code dữ liệu nếu chức năng cần lấy từ backend.**
7. **Không thay đổi cấu trúc project nếu không thật sự cần thiết.**
8. **Mỗi lần sửa phải có mục tiêu rõ ràng.**
9. **Sau khi sửa phải kiểm tra bằng lệnh chạy phù hợp.**
10. **Nếu chạy thành công thì ghi log vào file markdown.**
11. **Nếu chạy lỗi tối đa 2 lần thì phải dừng, phân tích lại nguyên nhân, lập kế hoạch mới rồi mới sửa tiếp.**
12. **Code mới phải dễ đọc, có comment giải thích các bước xử lý chính.**
13. **Method mới hoặc class mới phải có summary/comment mô tả chức năng.**
14. **Không được báo thành công nếu chưa chạy kiểm tra hoặc chưa có bằng chứng rằng chức năng hoạt động.**

---

## 2. Quy trình làm việc tổng quát

Mọi task đều phải đi theo quy trình tổng quát sau:

```text
Bước 1: Nhận yêu cầu từ người dùng
Bước 2: Phân tích yêu cầu
Bước 3: Xác định loại task
Bước 4: Lập kế hoạch sửa chi tiết
Bước 5: Thực hiện code theo đúng phạm vi
Bước 6: Chạy kiểm tra
Bước 7: Nếu lỗi thì sửa theo giới hạn số lần chạy
Bước 8: Nếu thành công thì ghi log vào markdown
Bước 9: Báo cáo kết quả cho người dùng
```

---

## 3. Phân loại task

AI phải tự phân loại task vào một trong các trường hợp sau:

| Loại task | Khi nào dùng |
|---|---|
| Trường hợp 1: Lỗi frontend | Khi giao diện bị lỗi, UI hiển thị sai, state sai, form sai, dữ liệu render sai, CSS/layout sai |
| Trường hợp 2: Nối endpoint vào frontend | Khi frontend cần gọi API backend để thực hiện một chức năng |
| Trường hợp 3: Thêm endpoint backend | Khi backend chưa có endpoint cần thiết hoặc endpoint hiện tại không đáp ứng nghiệp vụ |
| Trường hợp 4: Sửa lỗi nhỏ không thuộc 3 nhóm trên | Chỉ dùng khi task rất nhỏ, ví dụ sửa typo, đổi text, sửa route đơn giản |

Nếu không chắc task thuộc loại nào, AI phải nói rõ đang phân vân ở đâu và hỏi lại người dùng trước khi sửa.

---

# 4. Trường hợp 1: Quy trình sửa lỗi frontend

## 4.1. Khi nào áp dụng

Áp dụng khi người dùng yêu cầu sửa các lỗi như:

- Giao diện hiển thị sai.
- Button không hoạt động.
- Form không submit được.
- Component không render dữ liệu.
- CSS/layout bị lệch.
- State bị sai.
- Trang bị crash.
- React/Vite báo lỗi frontend.
- Gọi API đã có nhưng xử lý dữ liệu frontend sai.

---

## 4.2. Quy trình bắt buộc

```text
Bước 1: Đọc yêu cầu người dùng
Bước 2: Xác định lỗi thuộc component/page nào
Bước 3: Kiểm tra các file frontend liên quan
Bước 4: Lập kế hoạch sửa chi tiết
Bước 5: Chỉ sửa các file frontend cần thiết
Bước 6: Chạy npm install
Bước 7: Chạy npm run dev
Bước 8: Nếu lỗi thì sửa tối đa 2 lần
Bước 9: Nếu chạy được thì ghi log vào markdown
Bước 10: Báo cáo thành công cho người dùng
```

---

## 4.3. Kế hoạch trước khi code phải có

Trước khi sửa frontend, AI phải ghi rõ:

```markdown
## Kế hoạch sửa frontend

1. Lỗi hiện tại:
   - Mô tả ngắn gọn lỗi người dùng đang gặp.

2. File dự kiến kiểm tra:
   - `path/to/file1`
   - `path/to/file2`

3. Nguyên nhân dự đoán:
   - Nêu nguyên nhân có khả năng cao nhất.

4. Cách sửa dự kiến:
   - Sửa state / props / API call / CSS / route / form validation...

5. Cách kiểm tra:
   - Chạy `npm install`
   - Chạy `npm run dev`
   - Kiểm tra màn hình/chức năng liên quan
```

---

## 4.4. Lệnh kiểm tra bắt buộc

Sau khi sửa frontend, chạy đúng thứ tự:

```bash
npm install
npm run dev
```

Nếu project có thư mục frontend riêng, phải `cd` vào đúng thư mục trước khi chạy:

```bash
cd frontend
npm install
npm run dev
```

---

## 4.5. Ghi log sau khi thành công

Nếu frontend chạy được, AI phải ghi thêm một dòng vào file markdown log, ví dụ:

```markdown
## Lịch sử chỉnh sửa

1. [Frontend] Sửa lỗi form đăng nhập không submit được
   - Mục đích: Cho phép người dùng đăng nhập từ giao diện.
   - File đã chỉnh sửa:
     - `src/pages/LoginPage.jsx`
     - `src/services/authService.js`
   - Cách kiểm tra:
     - Đã chạy `npm install`
     - Đã chạy `npm run dev`
   - Kết quả: Thành công.
```

Tên file log đề xuất:

```text
AI_CHANGELOG.md
```

---

# 5. Trường hợp 2: Quy trình nối endpoint vào frontend

## 5.1. Khi nào áp dụng

Áp dụng khi người dùng yêu cầu frontend thực hiện chức năng bằng API backend, ví dụ:

- Đăng nhập / đăng ký.
- Lấy danh sách media.
- Upload bài hát.
- Like / unlike media.
- Tạo playlist.
- Thêm bài hát vào playlist.
- Follow / unfollow user.
- Xem thông báo.
- Tìm kiếm bài hát / album / playlist / user.

---

## 5.2. Quy trình bắt buộc

```text
Bước 1: Đọc yêu cầu chức năng từ người dùng
Bước 2: Xác định frontend cần làm gì
Bước 3: Xác định endpoint backend cần dùng
Bước 4: Kiểm tra endpoint hiện có trong backend
Bước 5: Nếu endpoint đã có thì dùng luôn
Bước 6: Nếu endpoint chưa có thì chuyển sang quy trình Trường hợp 3
Bước 7: Tạo hoặc cập nhật service API ở frontend
Bước 8: Nối service vào component/page
Bước 9: Xử lý loading, success, error rõ ràng
Bước 10: Chạy kiểm tra frontend
Bước 11: Nếu thành công thì ghi log vào markdown
Bước 12: Báo cáo kết quả cho người dùng
```

---

## 5.3. Kế hoạch trước khi code phải có

```markdown
## Kế hoạch nối endpoint

1. Chức năng cần làm:
   - Mô tả chức năng theo yêu cầu người dùng.

2. Frontend cần xử lý:
   - Trang/component nào gọi API.
   - Dữ liệu gửi lên backend.
   - Dữ liệu nhận về từ backend.
   - Cách hiển thị loading/error/success.

3. Endpoint dự kiến sử dụng:
   - Method: `GET | POST | PUT | DELETE`
   - URL: `/api/...`
   - Body/Query/Params cần truyền.

4. File backend cần kiểm tra:
   - Controller liên quan.
   - DTO liên quan.
   - Service/Feature/Repository liên quan nếu cần.

5. File frontend dự kiến chỉnh sửa:
   - `src/services/...`
   - `src/pages/...`
   - `src/components/...`

6. Cách kiểm tra:
   - Chạy frontend bằng `npm install` và `npm run dev`
   - Test thao tác trên UI
   - Kiểm tra request/response nếu cần
```

---

## 5.4. Luật kiểm tra endpoint

Trước khi tạo endpoint mới, AI bắt buộc phải kiểm tra backend hiện tại:

1. Tìm trong các controller hiện có.
2. Tìm trong route `/api/...` liên quan.
3. Kiểm tra DTO request/response nếu có.
4. Kiểm tra service/feature/repository đã hỗ trợ nghiệp vụ chưa.
5. Chỉ tạo endpoint mới khi chắc chắn backend chưa có endpoint phù hợp.

---

## 5.5. Nếu endpoint đã có

Nếu backend đã có endpoint phù hợp:

```text
Không tạo endpoint mới.
Không sửa backend nếu không cần.
Chỉ nối frontend vào endpoint có sẵn.
```

Frontend nên có service riêng, ví dụ:

```text
src/services/mediaService.js
src/services/playlistService.js
src/services/authService.js
src/services/userService.js
```

---

## 5.6. Nếu endpoint chưa có

Nếu backend chưa có endpoint phù hợp:

```text
Dừng quy trình nối frontend.
Chuyển sang Trường hợp 3: Thêm endpoint backend.
Sau khi backend chạy được mới quay lại nối frontend.
```

---

# 6. Trường hợp 3: Quy trình thêm endpoint backend

## 6.1. Khi nào áp dụng

Áp dụng khi:

- Backend chưa có endpoint cần thiết.
- Endpoint hiện tại thiếu nghiệp vụ quan trọng.
- Frontend cần một API mới để hoàn thành chức năng.
- Controller chưa có route phù hợp.
- Repository/service chưa có method xử lý nghiệp vụ.

---

## 6.2. Nguyên tắc backend bắt buộc

Khi thêm endpoint backend:

1. **Không chỉnh sửa file không liên quan.**
2. **Không phá vỡ endpoint cũ.**
3. **Không đổi response format của endpoint cũ nếu frontend đang dùng.**
4. **Không bỏ qua phân quyền nếu chức năng cần JWT/user hiện tại.**
5. **Không tự ý public dữ liệu riêng tư.**
6. **Repository chỉ xử lý truy vấn dữ liệu.**
7. **Feature/Service xử lý nghiệp vụ.**
8. **Controller chỉ nhận request, gọi feature/service, trả response.**
9. **DTO dùng để nhận/trả dữ liệu, không trả trực tiếp entity nếu không phù hợp.**
10. **Method mới phải có comment/summary mô tả chức năng.**

---

## 6.3. Thứ tự code backend bắt buộc

Khi thêm endpoint backend, AI phải làm theo thứ tự:

```text
Bước 1: Phân tích nghiệp vụ
Bước 2: Kiểm tra endpoint hiện có
Bước 3: Xác định DTO cần dùng hoặc cần tạo
Bước 4: Viết repository method
Bước 5: Viết feature/service method
Bước 6: Viết controller endpoint
Bước 7: Kiểm tra build/run backend
Bước 8: Nếu chạy được thì ghi log vào markdown
Bước 9: Báo cáo kết quả
```

---

## 6.4. Kế hoạch trước khi code backend phải có

```markdown
## Kế hoạch thêm endpoint backend

1. Chức năng cần thêm:
   - Mô tả nghiệp vụ cần xử lý.

2. Endpoint dự kiến:
   - Method: `GET | POST | PUT | DELETE`
   - URL: `/api/...`
   - Quyền truy cập: Public / cần đăng nhập / chỉ chủ sở hữu / admin

3. Dữ liệu đầu vào:
   - Route params
   - Query params
   - Request body
   - UserId từ JWT nếu cần

4. Dữ liệu đầu ra:
   - Response DTO
   - Status code dự kiến

5. File dự kiến chỉnh sửa:
   - Repository
   - Feature/Service
   - Controller
   - DTO nếu cần

6. Cách kiểm tra:
   - Build backend
   - Run backend
   - Test endpoint bằng Swagger/Postman/curl nếu có thể
```

---

## 6.5. Cấu trúc trách nhiệm backend

### Repository

Repository chỉ nên làm:

- Query database.
- Insert/update/delete dữ liệu.
- Kiểm tra dữ liệu tồn tại.
- Không chứa logic giao diện.
- Không trả dữ liệu dư thừa.

Ví dụ comment cần có:

```csharp
/// <summary>
/// Kiểm tra playlist có thuộc về user hiện tại hay không.
/// </summary>
```

---

### Feature/Service

Feature/Service nên làm:

- Kiểm tra nghiệp vụ.
- Kiểm tra quyền truy cập.
- Gọi repository.
- Map dữ liệu sang DTO.
- Quyết định thông báo lỗi phù hợp.

Ví dụ comment trong method:

```csharp
// Bước 1: Kiểm tra media có tồn tại không
// Bước 2: Kiểm tra user hiện tại có quyền thao tác không
// Bước 3: Thực hiện thêm media vào playlist
// Bước 4: Trả kết quả về controller
```

---

### Controller

Controller chỉ nên làm:

- Nhận request.
- Lấy route params/query/body.
- Lấy userId từ JWT nếu cần.
- Gọi feature/service.
- Trả HTTP response.

Controller không nên chứa logic truy vấn database phức tạp.

---

## 6.6. Quy tắc phân quyền

Với các chức năng liên quan user hiện tại:

- Không bắt frontend truyền `currentUserId` nếu backend có thể lấy từ JWT.
- Với thao tác cá nhân, lấy userId từ token.
- Với thao tác trên dữ liệu của người khác, phải kiểm tra quyền.
- Không cho user sửa/xóa dữ liệu không thuộc về họ.

Ví dụ:

```text
POST /api/users/follow
```

Nên nhận:

```json
{
  "followeeId": 5
}
```

Không nên bắt frontend truyền:

```json
{
  "followerId": 1,
  "followeeId": 5
}
```

Vì `followerId` phải lấy từ JWT.

---

# 7. Luật chạy kiểm tra và xử lý lỗi

## 7.1. Giới hạn số lần chạy

AI được phép chạy kiểm tra tối đa **2 lần** cho mỗi vòng sửa.

Ví dụ frontend:

```bash
npm install
npm run dev
```

Ví dụ backend:

```bash
dotnet build
dotnet run
```

---

## 7.2. Nếu lần 1 bị lỗi

AI được phép sửa lỗi dựa trên log và chạy lại lần 2.

Trước khi sửa lại phải ghi ngắn gọn:

```markdown
## Phân tích lỗi lần 1

- Lỗi gặp phải:
- Nguyên nhân có khả năng:
- File cần sửa:
- Cách sửa:
```

---

## 7.3. Nếu lần 2 vẫn bị lỗi

AI không được tiếp tục sửa mò.

Phải dừng lại và làm lại quy trình:

```text
Dừng sửa code
Phân tích lại toàn bộ lỗi
Lập kế hoạch mới
Chỉ tiếp tục khi đã có hướng xử lý rõ ràng
```

Báo cáo cho người dùng:

```markdown
Hiện tại đã thử sửa và chạy kiểm tra 2 lần nhưng vẫn còn lỗi.
Tôi sẽ dừng sửa mò và quay lại bước phân tích.

Lỗi còn lại:
- ...

Hướng xử lý mới đề xuất:
- ...
```

---

# 8. Quy tắc comment và summary trong code

## 8.1. Với method mới

Method mới phải có summary nếu là C#:

```csharp
/// <summary>
/// Tạo playlist mới cho user hiện tại.
/// </summary>
/// <param name="request">Thông tin playlist cần tạo.</param>
/// <returns>Thông tin playlist sau khi tạo thành công.</returns>
```

Trong thân method nên có comment từng bước chính:

```csharp
// Bước 1: Lấy userId hiện tại từ JWT
// Bước 2: Validate dữ liệu request
// Bước 3: Gọi service để tạo playlist
// Bước 4: Trả response về client
```

---

## 8.2. Với class mới

Class mới phải có summary:

```csharp
/// <summary>
/// DTO dùng để nhận dữ liệu tạo playlist từ frontend.
/// </summary>
public class CreatePlaylistRequest
{
}
```

---

## 8.3. Với frontend

Frontend nên comment các đoạn xử lý chính, đặc biệt là:

- Gọi API.
- Xử lý submit form.
- Xử lý loading/error.
- Map dữ liệu từ backend sang UI.

Ví dụ:

```javascript
// Bước 1: Bật trạng thái loading khi bắt đầu gọi API
// Bước 2: Gửi request đăng nhập lên backend
// Bước 3: Lưu token nếu đăng nhập thành công
// Bước 4: Hiển thị lỗi nếu backend trả lỗi
```

Không cần comment quá nhiều cho các dòng code hiển nhiên.

---

# 9. Quy tắc ghi file markdown log

## 9.1. Tên file log đề xuất

```text
AI_CHANGELOG.md
```

Nếu project đã có file log khác, dùng file hiện có theo yêu cầu người dùng.

---

## 9.2. Format log bắt buộc

Mỗi task hoàn thành phải ghi theo mẫu:

```markdown
## Lịch sử chỉnh sửa

1. [Loại task] Tên công việc
   - Mục đích: Công việc này dùng để làm gì.
   - File đã chỉnh sửa:
     - `path/to/file1`
     - `path/to/file2`
   - File đã thêm mới:
     - `path/to/new-file` nếu có
   - Endpoint liên quan:
     - `GET /api/...` nếu có
   - Cách kiểm tra:
     - Lệnh đã chạy
   - Kết quả: Thành công / Còn lỗi
   - Ghi chú: Thông tin bổ sung nếu cần
```

---

## 9.3. Quy tắc đánh số thứ tự

- Log phải đánh số tăng dần.
- Không ghi đè log cũ.
- Không xóa lịch sử cũ.
- Nếu chưa có file log thì tạo mới.
- Nếu đã có file log thì ghi thêm vào cuối file.

---

# 10. Quy tắc hỏi lại khi thiếu dữ liệu

AI phải hỏi lại người dùng nếu thiếu các thông tin như:

- Endpoint cần dùng chưa rõ.
- Chức năng nghiệp vụ chưa rõ.
- Dữ liệu request body chưa rõ.
- Quyền truy cập chưa rõ.
- UI mong muốn chưa rõ.
- Dữ liệu mẫu chưa có.
- Người dùng yêu cầu thêm dữ liệu nhưng không nói rõ nội dung.

AI không được tự bịa dữ liệu như:

- UserId.
- MediaId.
- PlaylistId.
- Role.
- Token.
- Email/password mẫu.
- Dữ liệu database.

Mẫu hỏi lại:

```markdown
Mình đang thiếu thông tin để làm đúng chức năng này:

1. ...
2. ...

Bạn xác nhận giúp mình trước khi mình sửa code nhé.
```

---

# 11. Quy tắc báo cáo kết quả cho người dùng

Sau khi hoàn thành, AI phải báo cáo ngắn gọn theo mẫu:

```markdown
## Kết quả

Đã hoàn thành: ...

File đã chỉnh sửa:
- `...`
- `...`

File đã thêm mới:
- `...`

Endpoint đã dùng/thêm:
- `...`

Đã kiểm tra:
- `npm install`
- `npm run dev`
- hoặc `dotnet build`, `dotnet run`

Kết quả kiểm tra:
- Thành công / Còn lỗi

Log đã ghi vào:
- `AI_CHANGELOG.md`
```

Không được nói “xong rồi” nếu chưa ghi rõ đã sửa gì và kiểm tra như thế nào.

---

# 12. Quy trình mẫu hoàn chỉnh

## 12.1. Ví dụ sửa frontend

```text
Người dùng yêu cầu: Sửa lỗi nút Like không đổi trạng thái sau khi bấm.

AI phải làm:
1. Kiểm tra component hiển thị nút Like.
2. Kiểm tra state like hiện tại.
3. Kiểm tra service gọi API like/unlike.
4. Lập kế hoạch sửa.
5. Sửa frontend.
6. Chạy npm install.
7. Chạy npm run dev.
8. Nếu chạy được, ghi log vào AI_CHANGELOG.md.
9. Báo cáo thành công.
```

---

## 12.2. Ví dụ nối endpoint

```text
Người dùng yêu cầu: Nối chức năng tạo playlist từ frontend.

AI phải làm:
1. Kiểm tra frontend form tạo playlist.
2. Kiểm tra backend đã có POST /api/playlists chưa.
3. Nếu có, dùng endpoint đó.
4. Nếu chưa có, chuyển sang quy trình thêm endpoint backend.
5. Tạo playlistService ở frontend nếu chưa có.
6. Gọi API khi submit form.
7. Xử lý loading/error/success.
8. Chạy kiểm tra.
9. Ghi log.
10. Báo cáo.
```

---

## 12.3. Ví dụ thêm endpoint backend

```text
Người dùng yêu cầu: Thêm API lấy danh sách follower của user.

AI phải làm:
1. Kiểm tra controller user đã có endpoint chưa.
2. Nếu chưa có, xác định route cần thêm.
3. Viết repository lấy follower từ database.
4. Viết feature/service xử lý phân quyền và map DTO.
5. Viết controller trả response.
6. Build backend.
7. Run backend.
8. Ghi log.
9. Báo cáo.
```

---

# 13. Checklist cuối trước khi báo thành công

Trước khi báo thành công, AI phải tự kiểm tra:

```markdown
- [ ] Đã đọc kỹ yêu cầu người dùng
- [ ] Đã lập kế hoạch trước khi code
- [ ] Chỉ sửa file liên quan
- [ ] Không tự đoán dữ liệu
- [ ] Không hard-code dữ liệu không cần thiết
- [ ] Có comment cho logic mới
- [ ] Có summary cho method/class mới nếu thêm backend
- [ ] Đã chạy kiểm tra phù hợp
- [ ] Nếu lỗi, đã không chạy quá 2 lần trong một vòng sửa
- [ ] Đã ghi log vào markdown
- [ ] Đã báo cáo rõ file đã sửa và kết quả kiểm tra
```

---

# 14. Prompt mẫu để đưa cho AI khác

Có thể copy đoạn dưới đây làm prompt chính cho AI khi sửa project:

```markdown
Bạn đang làm việc trên project TuneVault ở giai đoạn cuối. Backend đã có nhiều endpoint cơ bản và frontend đã nối một vài endpoint. Nhiệm vụ của bạn là sửa lỗi hoặc hoàn thiện chức năng theo quy trình chuẩn, không sửa lan man.

Trước khi code, bạn bắt buộc phải:
1. Đọc kỹ yêu cầu.
2. Phân loại task: lỗi frontend, nối endpoint, hoặc thêm endpoint backend.
3. Lập kế hoạch chi tiết.
4. Chỉ sửa các file liên quan.
5. Không tự đoán dữ liệu nếu thiếu thông tin.

Nếu là lỗi frontend:
- Kiểm tra component/page/service liên quan.
- Sửa frontend.
- Chạy `npm install` và `npm run dev`.
- Nếu chạy được, ghi log vào `AI_CHANGELOG.md`.

Nếu là nối endpoint:
- Kiểm tra backend đã có endpoint phù hợp chưa.
- Nếu có thì dùng endpoint đó.
- Nếu chưa có thì chuyển sang quy trình thêm endpoint backend.
- Sau khi nối frontend, chạy kiểm tra và ghi log.

Nếu là thêm endpoint backend:
- Không chỉnh sửa file không liên quan.
- Viết repository trước.
- Viết feature/service xử lý nghiệp vụ.
- Viết controller cuối cùng.
- Method/class mới phải có summary.
- Trong method phải có comment các bước xử lý chính.
- Build/run backend để kiểm tra.
- Nếu thành công, ghi log vào `AI_CHANGELOG.md`.

Luật xử lý lỗi:
- Mỗi vòng sửa chỉ được chạy kiểm tra tối đa 2 lần.
- Nếu sau 2 lần vẫn lỗi, dừng sửa mò.
- Quay lại phân tích lỗi và lập kế hoạch mới.

Sau khi hoàn thành, báo cáo rõ:
- Đã làm gì.
- Đã sửa file nào.
- Đã thêm file nào.
- Đã dùng/thêm endpoint nào.
- Đã chạy lệnh kiểm tra nào.
- Kết quả kiểm tra ra sao.
- Log đã ghi vào đâu.
```

---

# 15. Gợi ý cấu trúc file log AI_CHANGELOG.md

```markdown
# AI Change Log - TuneVault

File này ghi lại các thay đổi do AI thực hiện trong giai đoạn hoàn thiện project.

---

## Lịch sử chỉnh sửa

1. [Frontend] Tên công việc
   - Mục đích:
   - File đã chỉnh sửa:
   - File đã thêm mới:
   - Endpoint liên quan:
   - Cách kiểm tra:
   - Kết quả:
   - Ghi chú:
```

---

## 16. Kết luận

AI chỉ được xem là hoàn thành task khi đáp ứng đủ 3 điều kiện:

1. **Code đúng phạm vi yêu cầu.**
2. **Đã chạy kiểm tra phù hợp.**
3. **Đã ghi log thay đổi vào markdown.**

Nếu thiếu một trong ba điều kiện trên, không được báo task đã hoàn thành.
