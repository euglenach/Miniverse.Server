// https://github.com/Cysharp/MagicOnion?tab=readme-ov-file#quick-start

using MagicOnion;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole().SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.MapMagicOnionService();

app.Run();