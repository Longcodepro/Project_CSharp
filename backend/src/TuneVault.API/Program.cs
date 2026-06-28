using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using TuneVault.Application.Common;
using TuneVault.Application.Common.Behaviors;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using TuneVault.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH SWAGGER
// =========================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500_000_000;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500_000_000;
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            ApiResponse<object?>.Fail(BuildModelStateMessage(context)));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TuneVault API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =========================================================================
// 2. ĐĂNG KÝ CÁC DỊCH VỤ CỐT LÕI
// =========================================================================
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetUserByIdQueryHandler).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(GetUserByIdQueryHandler).Assembly);

// =========================================================================
// 3. ĐĂNG KÝ DATABASE (DAPPER)
// =========================================================================
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("DatabaseOptions"));

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
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IFavoriteRepository,
                            TuneVault.Infrastructure.Repositories.FavoriteRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IFollowRepository,
                            TuneVault.Infrastructure.Repositories.FollowRepository>();
builder.Services.AddScoped<TuneVault.Application.Features.Friend.Abstractions.IFriendRepository,
                            TuneVault.Infrastructure.Repositories.FriendRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IPlayHistoryRepository,
                            TuneVault.Infrastructure.Repositories.PlayHistoryRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IMediaShareRepository,
                            TuneVault.Infrastructure.Repositories.MediaShareRepository>();
builder.Services.AddScoped<TuneVault.Application.Features.Share.Commands.ShareMedia.IMediaShareCommandRepository,
                            TuneVault.Infrastructure.Repositories.MediaShareRepository>();
builder.Services.AddScoped<TuneVault.Application.Features.Notification.Commands.INotificationCommandRepository,
                            TuneVault.Infrastructure.Repositories.NotificationRepository>();
builder.Services.AddScoped<TuneVault.Application.Features.Notification.Queries.INotificationQueryRepository,
                            TuneVault.Infrastructure.Repositories.NotificationRepository>();

// =========================================================================
// 5. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG
// =========================================================================
builder.Services.AddScoped<TuneVault.Application.Interfaces.IJwtTokenGenerator,
                            TuneVault.Infrastructure.Authentication.JwtTokenGenerator>();

builder.Services.AddScoped<TuneVault.Application.Abstractions.IEmailService,
                            TuneVault.Infrastructure.Services.GmailSmtpEmailService>();
builder.Services.AddScoped<IFileStorageService,
                            TuneVault.Infrastructure.Services.LocalFileStorageService>();

builder.Services.AddScoped<TuneVault.Application.Abstractions.ICurrentUserService,
                            TuneVault.Infrastructure.Services.CurrentUserService>();

builder.Services.AddScoped<TuneVault.Domain.Interfaces.ICurrentUserContext,
                            TuneVault.Infrastructure.Services.CurrentUserService>();
                            
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IPlaylistRepository,
                            TuneVault.Infrastructure.Repositories.PlaylistRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IAlbumRepository,
                            TuneVault.Infrastructure.Repositories.AlbumRepository>();
builder.Services.AddScoped<TuneVault.Domain.Interfaces.ICollectionLikeRepository,
                            TuneVault.Infrastructure.Repositories.CollectionLikeRepository>();

builder.Services.AddScoped<TuneVault.Domain.Interfaces.ISearchRepository,
                            TuneVault.Infrastructure.Repositories.SearchRepository>();
builder.Services.AddScoped<INotificationPusher,
                            TuneVault.Infrastructure.Realtime.SignalRNotificationPusher>();
builder.Services.AddSignalR();
// =========================================================================
// 6. CẤU HÌNH CORS & JWT
// =========================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",
                  "http://127.0.0.1:3000",
                  "http://localhost:5128",
                  "http://127.0.0.1:5128",
                  "http://localhost:5173",
                  "http://127.0.0.1:5173",
                  "http://localhost:5174",
                  "http://127.0.0.1:5174",
                  "http://localhost:5175",
                  "http://127.0.0.1:5175",
                  "http://localhost:5176",
                  "http://127.0.0.1:5176",
                  "http://localhost:5177",
                  "http://127.0.0.1:5177")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var jwtSecretKey = ReadRequiredConfiguration(
    builder.Configuration,
    "JwtSettings:SecretKey",
    "Không tìm thấy JWT SecretKey trong cấu hình.");
var jwtIssuer = ReadRequiredConfiguration(
    builder.Configuration,
    "JwtSettings:Issuer",
    "Không tìm thấy JWT Issuer trong cấu hình.");
var jwtAudience = ReadRequiredConfiguration(
    builder.Configuration,
    "JwtSettings:Audience",
    "Không tìm thấy JWT Audience trong cấu hình.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtIssuer,
        ValidateAudience         = true,
        ValidAudience            = jwtAudience,
        ClockSkew                = TimeSpan.FromSeconds(5),
        NameClaimType            = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName,
        RoleClaimType            = "role"
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            var cookieToken = context.Request.Cookies["tunevault_access_token"];

            if (!string.IsNullOrWhiteSpace(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(cookieToken))
            {
                context.Token = cookieToken;
            }

            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Fail("Bạn cần đăng nhập bằng JWT hợp lệ để thực hiện thao tác này. Vui lòng kiểm tra token hoặc đăng nhập lại."));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Fail("Tài khoản hiện tại không có quyền thực hiện thao tác này. Vui lòng kiểm tra vai trò hoặc quyền sở hữu tài nguyên."));
        }
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

        if (error is ValidationException)
        {
            var validationEx = (ValidationException)error;
            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Fail(BuildValidationMessage(validationEx)));
            return;
        }

        // DomainException → 400
        if (error is DomainException domainEx)
        {
            context.Response.StatusCode  = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            // Use ApiResponse.Fail for DomainException
            await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail(domainEx.Message));
            return;
        }

        // ForbiddenAccessException → 403
        if (error is TuneVault.Domain.Exceptions.ForbiddenAccessException forbiddenEx)
        {
            context.Response.StatusCode  = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            // Use ApiResponse.Fail for ForbiddenAccessException
            await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail(forbiddenEx.Message));
            return;
        }

        // UnauthorizedAccessException → 401
        if (error is UnauthorizedAccessException unauthorizedEx)
        {
            context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            // Use ApiResponse.Fail for UnauthorizedAccessException
            await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail(
                string.IsNullOrWhiteSpace(unauthorizedEx.Message)
                    ? "Bạn cần đăng nhập để thực hiện thao tác này."
                    : unauthorizedEx.Message));
            return;
        }

        // Exception chung → 500
        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var detail = app.Environment.IsDevelopment() ? error.Message : null;
        await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Fail(
            "Đã xảy ra lỗi không mong muốn khi xử lý yêu cầu. Vui lòng thử lại sau hoặc liên hệ quản trị viên nếu lỗi vẫn tiếp diễn.",
            detail));
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuneVault API v1"));

app.UseStaticFiles();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TuneVault.Infrastructure.Realtime.NotificationHub>("/hubs/notifications");
app.MapGet("/", () => Results.Ok(new
{
    success = true,
    message = "TuneVault API is running",
    environment = app.Environment.EnvironmentName
}));
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "TuneVault API" }))
   .WithName("Health");

app.Run();

static string BuildModelStateMessage(Microsoft.AspNetCore.Mvc.ActionContext context)
{
    var errors = context.ModelState
        .Where(entry => entry.Value?.Errors.Count > 0)
        .SelectMany(entry => entry.Value!.Errors.Select(error =>
        {
            var fieldName = NormalizeFieldName(entry.Key);
            var message = NormalizeModelBindingError(error.ErrorMessage);
            return string.IsNullOrWhiteSpace(fieldName)
                ? message
                : $"{fieldName}: {message}";
        }))
        .Where(message => !string.IsNullOrWhiteSpace(message))
        .Distinct()
        .Take(8)
        .ToList();

    return errors.Count == 0
        ? "Dữ liệu gửi lên không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc và định dạng dữ liệu."
        : $"Dữ liệu gửi lên không hợp lệ: {string.Join("; ", errors)}.";
}

static string BuildValidationMessage(ValidationException exception)
{
    var errors = exception.Errors
        .Where(error => error is not null)
        .Select(error => BuildValidationItemMessage(error))
        .Where(message => !string.IsNullOrWhiteSpace(message))
        .Distinct()
        .Take(8)
        .ToList();

    return errors.Count == 0
        ? "Dữ liệu gửi lên không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc và định dạng dữ liệu."
        : $"Dữ liệu gửi lên không hợp lệ: {string.Join("; ", errors)}.";
}

static string BuildValidationItemMessage(FluentValidation.Results.ValidationFailure failure)
{
    var fieldName = NormalizeFieldName(failure.PropertyName);
    var message = string.IsNullOrWhiteSpace(failure.ErrorMessage)
        ? "Giá trị không hợp lệ."
        : failure.ErrorMessage.Trim();

    return string.IsNullOrWhiteSpace(fieldName)
        ? message
        : $"{fieldName}: {message}";
}

static string NormalizeFieldName(string rawFieldName)
{
    if (string.IsNullOrWhiteSpace(rawFieldName))
        return string.Empty;

    var fieldName = rawFieldName.Trim();

    if (fieldName.StartsWith("$.", StringComparison.Ordinal))
        fieldName = fieldName[2..];

    return fieldName
        .Replace("$.", string.Empty, StringComparison.Ordinal)
        .Replace("[", ".", StringComparison.Ordinal)
        .Replace("]", string.Empty, StringComparison.Ordinal);
}

static string NormalizeModelBindingError(string rawMessage)
{
    if (string.IsNullOrWhiteSpace(rawMessage))
        return "Giá trị không hợp lệ.";

    var message = rawMessage.Trim();

    if (message.Contains("A non-empty request body is required", StringComparison.OrdinalIgnoreCase))
        return "Body request không được để trống.";

    if (message.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase))
        return "Kiểu dữ liệu không đúng hoặc không đúng định dạng mong đợi.";

    if (message.Contains("The value '' is invalid", StringComparison.OrdinalIgnoreCase))
        return "Giá trị không được để trống hoặc không đúng định dạng.";

    if (message.Contains("is required", StringComparison.OrdinalIgnoreCase))
        return "Trường này là bắt buộc.";

    if (message.Contains("not valid", StringComparison.OrdinalIgnoreCase))
        return "Giá trị không hợp lệ.";

    return message;
}

static string ReadRequiredConfiguration(IConfiguration configuration, string key, string errorMessage)
{
    var value = configuration[key];
    return !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException(errorMessage);
}
