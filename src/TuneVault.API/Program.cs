/// <summary>
/// Import namespace chứa DapperContext từ tầng Infrastructure
/// để Program.cs có thể nhận diện và đăng ký vào DI container
/// </summary>
using TuneVault.Infrastructure.DAO;

/// <summary>
/// Khởi tạo builder — đối tượng dùng để cấu hình toàn bộ ứng dụng
/// Tự động đọc appsettings.json và các biến môi trường
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Thêm 2 dòng này để dùng được Controller
builder.Services.AddControllers();

/// <summary>
/// Đăng ký DapperContext vào DI container với kiểu Singleton
/// Tức là chỉ tạo 1 instance duy nhất, dùng chung cho toàn bộ app
/// Phải đặt trước builder.Build() vì sau Build() thì không đăng ký được nữa
/// </summary>
///  Đăng ký DapperContext - chỉ tạo 1 lần duy nhất cho toàn bộ app
builder.Services.AddSingleton<DapperContext>();


/// <summary>
/// Đăng ký UserDAO vào DI container với kiểu Scoped.
/// Scoped nghĩa là mỗi HTTP request sẽ tạo 1 instance UserDAO mới,
/// dùng xuyên suốt request đó rồi tự hủy khi request kết thúc.
/// 
/// Tại sao dùng Scoped thay vì Singleton cho DAO?
/// - Singleton: 1 instance dùng chung cho TẤT CẢ request
///   → Nguy hiểm! Nếu 2 user cùng gọi API 1 lúc có thể bị xung đột dữ liệu
/// - Scoped: mỗi request có instance RIÊNG
///   → An toàn! Các request độc lập nhau hoàn toàn
/// 
/// Các DAO khác (SongDAO, PlaylistDAO, ...) cũng đăng ký tương tự:
/// builder.Services.AddScoped<SongDAO>();
/// builder.Services.AddScoped<PlaylistDAO>();
/// </summary>
builder.Services.AddScoped<UserDAO>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Thêm dòng này để app nhận diện được Controller
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "TuneVault API"
}));
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "TuneVault API"
}))
.WithName("Health");

app.Run();
