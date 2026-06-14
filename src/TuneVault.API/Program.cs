// API/Program.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Domain.Exceptions;
using TuneVault.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH SWAGGER
// =========================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TuneVault API", Version = "v1" });
});

// =========================================================================
// 2. ĐĂNG KÝ CÁC DỊCH VỤ CỐT LÕI
// =========================================================================
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetUserByIdQueryHandler).Assembly));

// =========================================================================
// 3. ĐĂNG KÝ DATABASE (DAPPER)
// =========================================================================
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("DatabaseOptions"));

// Chỉ đăng ký IDbConnectionFactory — KHÔNG đăng ký DapperContext (Rule 4.1)
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// =========================================================================
// 4. ĐĂNG KÝ REPOSITORIES
// =========================================================================
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IUserRepository,
                            TuneVault.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IOtpLogRepository,
                            TuneVault.Infrastructure.Repositories.OtpLogRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IMediaRepository,
                            TuneVault.Infrastructure.Repositories.MediaRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IAdminRepository,
                            TuneVault.Infrastructure.Repositories.AdminRepository>();

// =========================================================================
// 5. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG
// =========================================================================
builder.Services.AddScoped<TuneVault.Application.Interfaces.IJwtTokenGenerator,
                            TuneVault.Infrastructure.Authentication.JwtTokenGenerator>();

builder.Services.AddScoped<TuneVault.Application.Abstractions.IEmailService,
                            TuneVault.Infrastructure.Services.GmailSmtpEmailService>();

builder.Services.AddScoped<TuneVault.Application.Abstractions.ICurrentUserService,
                            TuneVault.Infrastructure.Services.CurrentUserService>();

// Register ICurrentUserContext (Domain level) — implement by CurrentUserService
builder.Services.AddScoped<TuneVault.Domain.Interfaces.ICurrentUserContext,
                            TuneVault.Infrastructure.Services.CurrentUserService>();

// =========================================================================
// 6. CẤU HÌNH CORS & JWT
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

// FIX: đọc từ "JwtSettings:SecretKey" để khớp với JwtTokenGenerator
// Đồng thời sync appsettings.json (xem ghi chú bên dưới)
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? builder.Configuration["Jwt:SecretKey"]       // fallback key cũ trong appsettings
    ?? throw new InvalidOperationException("Không tìm thấy JWT SecretKey trong cấu hình.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ClockSkew                = TimeSpan.Zero
    };
});

// =========================================================================
// 7. BUILD & MIDDLEWARE PIPELINE
// =========================================================================
var app = builder.Build();

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is null) return;

        var error = feature.Error;

        // DomainException → 400
        if (error is DomainException domainEx)
        {
            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                data    = (object?)null,
                message = domainEx.Message
            });
            return;
        }

        // ForbiddenAccessException → 403
        if (error is TuneVault.Domain.Exceptions.ForbiddenAccessException forbiddenEx)
        {
            context.Response.StatusCode  = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                data    = (object?)null,
                message = forbiddenEx.Message
            });
            return;
        }

        // UnauthorizedAccessException → 401
        if (error is UnauthorizedAccessException)
        {
            context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                data    = (object?)null,
                message = error.Message
            });
            return;
        }

        // Exception chung → 500
        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            data    = (object?)null,
            message = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
            detail  = app.Environment.IsDevelopment() ? error.Message : null
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuneVault API v1"));
}

app.UseCors("Frontend");
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "TuneVault API" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "TuneVault API" }))
   .WithName("Health");

app.Run();