// https://github.com/Cysharp/MagicOnion?tab=readme-ov-file#quick-start

using MagicOnion;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Miniverse.MagicOnion;
using Miniverse.ServerShared.Nats;
using NATS.Client.Hosting;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole().SetMinimumLevel(LogLevel.Debug);

// DI
AppLifetimeScope.Configure(builder.Services);

var app = builder.Build();
app.MapMagicOnionService();

var natsPubSub = app.Services.GetRequiredService<NatsPubSub>();
natsPubSub.Initialize(""); // todo: url設定

await app.RunAsync();