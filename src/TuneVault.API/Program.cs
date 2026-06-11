/// <summary>
/// Import namespace chứa DapperContext từ tầng Infrastructure
/// để Program.cs có thể nhận diện và đăng ký vào DI container
/// </summary>
using TuneVault.Infrastructure.DAO;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.Repositories;
using Dapper;
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
/// Đăng ký Repositories vào DI container với kiểu Scoped
/// Repository pattern tách biệt logic truy cập dữ liệu từ Business Logic
/// </summary>
builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
//test
builder.Services.AddEndpointsApiExplorer();  // ← có chưa?
builder.Services.AddSwaggerGen();             // ← có chưa?
// Giữ DAOs cho các chức năng khác
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<AlbumDAO>();
builder.Services.AddScoped<PlaylistDAO>();
builder.Services.AddScoped<SearchDAO>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
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
app.MapGet("/test-db", async (DapperContext context) =>
{
    try
    {
        using var conn = context.CreateConnection();
        var result = await conn.QuerySingleAsync<int>("SELECT 1");
        return Results.Ok(new { status = "Connected to SQL Server!" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});