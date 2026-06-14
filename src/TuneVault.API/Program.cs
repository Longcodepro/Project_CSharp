using TuneVault.API.Hubs;
using TuneVault.Infrastructure.DAO;
using TuneVault.Infrastructure.Repositories;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.Queries.GetNotifications;
using TuneVault.Application.Features.Follow.Commands;
using TuneVault.Application.Features.Favorite.Commands;
using TuneVault.Application.Features.Share.Commands.ShareMedia;
using TuneVault.Application.Features.Share.Queries.GetSharedWithMe;
using TuneVault.Application.Features.History.Commands;

var builder = WebApplication.CreateBuilder(args);

// Controller
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR realtime notification
builder.Services.AddSignalR();

// Database context
builder.Services.AddSingleton<DapperContext>();

// DAO cũ
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<AlbumDAO>();
builder.Services.AddScoped<PlaylistDAO>();
builder.Services.AddScoped<SearchDAO>();
builder.Services.AddScoped<InteractionDAO>();

// Notification mới
builder.Services.AddScoped<INotificationCommandRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationQueryRepository, NotificationRepository>();
builder.Services.AddScoped<MarkNotificationAsReadCommand>();
builder.Services.AddScoped<GetNotificationsQuery>();

// Follow
builder.Services.AddScoped<IFollowSqlRepository, FollowRepository>();
builder.Services.AddScoped<FollowUserCommand>();
builder.Services.AddScoped<UnFollowUserCommand>();

// Favorite
builder.Services.AddScoped<IFavoriteSqlRepository, FavoriteRepository>();
builder.Services.AddScoped<ToggleFavoriteCommand>();

builder.Services.AddScoped<IMediaShareCommandRepository, MediaShareRepository>();
builder.Services.AddScoped<IMediaShareQueryRepository, MediaShareRepository>();
builder.Services.AddScoped<ShareMediaCommand>();
builder.Services.AddScoped<GetSharedWithMeQuery>();

// Play History
builder.Services.AddScoped<IPlayHistorySqlRepository, PlayHistoryRepository>();
builder.Services.AddScoped<RecordPlayHistoryCommand>();

// CORS cho frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Middleware
app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// API controllers
app.MapControllers();

// SignalR hub endpoint
app.MapHub<NotificationHub>("/notificationHub")
   .RequireCors("Frontend");

// Test API
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