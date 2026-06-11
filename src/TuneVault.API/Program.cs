using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TuneVault.Application.Features.User.Queries.GetUserById;

var builder = WebApplication.CreateBuilder(args);

// Kích hoạt bộ điều khiển API Controller
builder.Services.AddControllers();

// =========================================================================
// 1. CẤU HÌNH SWAGGER (GIAO DIỆN TEST API)
// =========================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================================================================
// 2. ĐĂNG KÝ CÁC DỊCH VỤ CỐT LÕI (SYSTEM SERVICES)
// =========================================================================
builder.Services.AddHttpContextAccessor();

// Đăng ký MediatR để tự động quét và kích hoạt các Handler xử lý logic
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(GetUserByIdQueryHandler).Assembly));

// =========================================================================
// 3. ĐĂNG KÝ DATABASE KẾT NỐI & REPOSITORY (DAPPER)
// =========================================================================
// Đăng ký Context quản lý kết nối Dapper
builder.Services.AddSingleton<TuneVault.Infrastructure.Persistence.DapperContext>(); 

// Đăng ký UserRepository (Ép cổng tuyệt đối tránh lỗi thiếu dùng hoặc trùng tên)
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IUserRepository, TuneVault.Infrastructure.Repositories.UserRepository>();

// =========================================================================
// 4. ĐĂNG KÝ CÁC DỊCH VỤ XÁC THỰC BỔ SUNG
// =========================================================================
builder.Services.AddScoped<TuneVault.Application.Abstractions.ITokenService, TuneVault.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<TuneVault.Application.Abstractions.ICurrentUserService, TuneVault.Infrastructure.Services.CurrentUserService>();

// =========================================================================
// 5. CẤU HÌNH CORS & JWT AUTHENTICATION
// =========================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtSecretKey = "Chuoi_Secret_Key_Sieu_Bao_Mat_Cua_TuneVault_2026"; 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero 
    };
});

// =========================================================================
// 6. XÂY DỰNG APP & THIẾT LẬP MIDDLEWARE PIPELINE
// =========================================================================
var app = builder.Build();

// Bật giao diện Swagger trực quan khi chạy ở môi trường phát triển (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuneVault API v1");
    });
}

app.UseCors("Frontend");
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Luồng xử lý bảo mật bắt buộc (Xác thực trước -> Phân quyền sau) // tắt tạm thời
// app.UseAuthentication(); 
// app.UseAuthorization();  

app.MapControllers();

// Các Endpoint kiểm tra nhanh trạng thái hoạt động hệ thống
app.MapGet("/", () => Results.Ok(new { service = "TuneVault API" }));

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "TuneVault API"
})).WithName("Health");

app.Run();