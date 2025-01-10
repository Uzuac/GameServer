using GameServer.Infrastructure.Services;
using GameServer.Domain.Interfaces;
using GameServer.Application.Services;
using GameServer.Domain;
using Serilog;
using System.Runtime.InteropServices;
using GameServer.Application.Routing;
using GameServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GameServer.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

static void SeedData(AppDbContext context)
{
    context.Players.Add(new GameServer.Domain.Model.Player()
    {
        Id = Guid.Parse("0287c93e-b236-4c99-befa-22cdfba3e976"),
        Devices = new List<Device> { new Device() { Id = Guid.Parse("62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f") } },
        Name = "Player1",
        Resources = new List<Resource>()
    });

    context.Players.Add(new GameServer.Domain.Model.Player()
    {
        Id = Guid.Parse("8c64bc82-9db8-43ec-b310-09614dfe2395"),
        Devices = new List<Device> { new Device() { Id = Guid.Parse("803efc45-43fc-4266-823e-085a1350a230") } },
        Name = "Player2",
        Resources = new List<Resource>()
    });

    context.SaveChanges();
}

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRequestRouter, RequestRouter>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
//builder.Services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>()!);
builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddApplicationServices();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData(context);
}

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
};

app.UseWebSockets(webSocketOptions);

//SetupInMemoryDb();

app.Map("/ws", async (HttpContext httpContext, string deviceId, IConnectionManager connectionManager, IRequestRouter requestRouter) =>
{
    if (!httpContext.WebSockets.IsWebSocketRequest)
    {
        httpContext.Response.StatusCode = 400;
        await httpContext.Response.WriteAsync("Expected a WebSocket request");

        return;
    }

    var socket = await httpContext.WebSockets.AcceptWebSocketAsync();

    await requestRouter.RunCommandHandler(deviceId, socket);
    return;
});

app.MapFallback(async (HttpContext httpContext) =>
{
    httpContext.Response.StatusCode = 404;
    await httpContext.Response.WriteAsync("Only WebSocket requests are accepted");
});

app.Run();

