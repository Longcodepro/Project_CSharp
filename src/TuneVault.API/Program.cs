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
// Bind DatabaseOptions từ appsettings.json
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("DatabaseOptions"));

// Đăng ký factory và context
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<DapperContext>();

// =========================================================================
// 4. ĐĂNG KÝ REPOSITORIES
// =========================================================================
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IUserRepository,
                            TuneVault.Infrastructure.Repositories.UserRepository>();

builder.Services.AddScoped<TuneVault.Domain.Interfaces.IMediaRepository,
                            TuneVault.Infrastructure.Repositories.MediaRepository>();

// =========================================================================
// 5. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG
// =========================================================================
builder.Services.AddScoped<TuneVault.Application.Abstractions.ITokenService,
                            TuneVault.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<TuneVault.Application.Abstractions.ICurrentUserService,
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

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Không tìm thấy 'Jwt:SecretKey' trong appsettings.json.");

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
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ClockSkew                = TimeSpan.Zero
    };
});

// =========================================================================
// 7. BUILD & MIDDLEWARE PIPELINE
// =========================================================================
var app = builder.Build();

// --- Global Exception Handler ---
// Bắt DomainException → 400 Bad Request
// Bắt Exception chung → 500 Internal Server Error
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is null) return;

        var error = feature.Error;

        // DomainException: lỗi nghiệp vụ → 400
        if (error is DomainException domainEx)
        {
            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 400,
                message    = domainEx.Message
            });
            return;
        }

        // Exception chung → 500
        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = 500,
            message    = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
            detail     = app.Environment.IsDevelopment() ? error.Message : null
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

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { service = "TuneVault API" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "TuneVault API" }))
   .WithName("Health");

app.Run();
