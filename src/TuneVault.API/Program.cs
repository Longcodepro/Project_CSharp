/// <summary>
/// Import namespace chứa các Interface từ tầng Domain
/// để Program.cs có thể đăng ký Repository theo đúng interface.
/// 
/// Ví dụ:
/// IAlbumRepository -> AlbumRepository
/// IPlaylistRepository -> PlaylistRepository
/// ISearchRepository -> SearchRepository
/// </summary>
using TuneVault.Domain.Interfaces;

/// <summary>
/// Import namespace chứa DapperContext và các DAO từ tầng Infrastructure
/// để Program.cs có thể nhận diện và đăng ký vào DI container.
/// 
/// Các DAO hiện đang dùng:
/// - UserDAO
/// - AlbumDAO
/// - PlaylistDAO
/// - SearchDAO
/// </summary>
using TuneVault.Infrastructure.DAO;

/// <summary>
/// Import namespace chứa các Repository từ tầng Infrastructure.
/// 
/// Repository là nơi implement phần thân hàm đã khai báo trong Interface.
/// Đây là phần nhóm trưởng yêu cầu viết code ở Infrastructure.
/// </summary>
using TuneVault.Infrastructure.Repositories;

/// <summary>
/// Khởi tạo builder — đối tượng dùng để cấu hình toàn bộ ứng dụng.
/// 
/// Builder sẽ tự động đọc:
/// - appsettings.json
/// - appsettings.Development.json
/// - biến môi trường
/// - cấu hình chạy project
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Đăng ký Controller vào service container.
/// 
/// Nếu không có dòng này thì các file Controller như:
/// - AlbumController
/// - PlaylistController
/// - SearchController
/// sẽ không hoạt động.
/// </summary>
builder.Services.AddControllers();

/// <summary>
/// Đăng ký DapperContext vào DI container với kiểu Singleton.
/// 
/// Singleton nghĩa là chỉ tạo 1 instance duy nhất,
/// dùng chung cho toàn bộ vòng đời ứng dụng.
/// 
/// DapperContext chủ yếu dùng để tạo kết nối database,
/// nên để Singleton là hợp lý.
/// 
/// Phải đăng ký trước builder.Build(),
/// vì sau khi Build() thì không thể đăng ký service nữa.
/// </summary>
builder.Services.AddSingleton<DapperContext>();

/// <summary>
/// Đăng ký các DAO vào DI container với kiểu Scoped.
/// 
/// Scoped nghĩa là mỗi HTTP request sẽ tạo 1 instance DAO mới,
/// dùng xuyên suốt request đó rồi tự hủy khi request kết thúc.
/// 
/// Tại sao dùng Scoped thay vì Singleton cho DAO?
/// - Singleton: 1 instance dùng chung cho tất cả request
///   dễ gây xung đột nếu nhiều user gọi API cùng lúc.
/// - Scoped: mỗi request có instance riêng
///   an toàn hơn khi xử lý dữ liệu.
/// </summary>
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<AlbumDAO>();
builder.Services.AddScoped<PlaylistDAO>();
builder.Services.AddScoped<SearchDAO>();

/// <summary>
/// Đăng ký Repository cho phần nhiệm vụ của mình.
/// 
/// Phần của mình gồm:
/// 1. Album Playlist
///    - Tạo / sửa / xóa album
///    - Thêm / xóa / sắp xếp bài trong album
///    - Tạo / sửa / xóa playlist
///    - Thêm / xóa / sắp xếp bài trong playlist
///    - Đặt playlist / album công khai hoặc riêng tư
/// 
/// 2. Tìm kiếm & Khám phá
///    - Tìm kiếm bài hát / podcast
///    - Tìm kiếm nghệ sĩ
///    - Tìm kiếm album / playlist
///    - Lọc theo thể loại / genre
///    - Trending bài nghe nhiều nhất
/// 
/// Lưu ý:
/// Không đăng ký Favorite / Follow / History ở đây
/// vì đó không phải phần nhiệm vụ của mình.
/// </summary>
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();

/// <summary>
/// Cấu hình CORS để frontend có thể gọi API backend.
/// 
/// Nếu frontend chạy bằng React thường:
/// - http://localhost:3000
/// - http://127.0.0.1:3000
/// 
/// Nếu frontend chạy bằng Vite:
/// - http://localhost:5173
/// - http://127.0.0.1:5173
/// 
/// Nếu không có CORS, frontend sẽ bị lỗi khi gọi API khác port.
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

/// <summary>
/// Build ứng dụng sau khi đã cấu hình xong toàn bộ service.
/// 
/// Sau dòng này, app bắt đầu chuyển sang phần cấu hình middleware.
/// </summary>
var app = builder.Build();

/// <summary>
/// Bật CORS policy tên là "Frontend".
/// 
/// Dòng này phải đặt trước app.MapControllers()
/// để các API controller nhận được cấu hình CORS.
/// </summary>
app.UseCors("Frontend");

/// <summary>
/// Cho phép backend phục vụ file tĩnh nếu project có dùng wwwroot.
/// 
/// Ví dụ:
/// - ảnh
/// - file audio
/// - file css/js tĩnh
/// </summary>
app.UseStaticFiles();

/// <summary>
/// Nếu không phải môi trường Development thì bật HTTPS Redirection.
/// 
/// Khi đang code local, thường app.Environment.IsDevelopment() sẽ là true,
/// nên dòng app.UseHttpsRedirection() sẽ không chạy.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

/// <summary>
/// Map toàn bộ Controller API.
/// 
/// Nhờ dòng này, các controller có route như:
/// - /api/album
/// - /api/playlist
/// - /api/search
/// mới có thể được gọi từ Postman hoặc frontend.
/// </summary>
app.MapControllers();

/// <summary>
/// API mặc định để kiểm tra backend có chạy hay không.
/// 
/// Khi mở trình duyệt vào URL gốc,
/// ví dụ http://localhost:xxxx/
/// sẽ trả về thông tin service.
/// </summary>
app.MapGet("/", () => Results.Ok(new
{
    service = "TuneVault API"
}));

/// <summary>
/// API health check.
/// 
/// Dùng để kiểm tra nhanh backend còn sống hay không.
/// 
/// Gọi:
/// GET /health
/// </summary>
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "TuneVault API"
}))
.WithName("Health");

/// <summary>
/// Chạy ứng dụng.
/// 
/// Đây là dòng cuối cùng của Program.cs.
/// </summary>
app.Run();