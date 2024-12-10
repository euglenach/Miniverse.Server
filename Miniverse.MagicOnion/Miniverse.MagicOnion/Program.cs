// https://github.com/Cysharp/MagicOnion?tab=readme-ov-file#quick-start

using MagicOnion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Miniverse.MagicOnion;
using Miniverse.ServerShared;
using Miniverse.ServerShared.Nats;
using NATS.Client.Hosting;
using ZLogger;

MessagePackOptionRegister.Register();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole().SetMinimumLevel(LogLevel.Debug);

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole(options =>
{
    options.UsePlainTextFormatter(formatter =>
    {
        formatter.SetPrefixFormatter($"{0:local-longdate} [{1:short}] ", (in MessageTemplate template, in LogInfo info) =>
                                         template.Format(info.Timestamp, info.LogLevel));
    });
}).SetMinimumLevel(LogLevel.Debug);

// DI
AppLifetimeScope.Configure(builder.Services);

builder.WebHost.UseKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpoint =>
    {
        endpoint.Protocols = HttpProtocols.Http2;
    });
    if(Environment.GetEnvironmentVariable("PORT") is {} port && int.TryParse(port, out var result))
    {
        options.ListenAnyIP(result);
    }
});

var app = builder.Build();
app.MapMagicOnionService();

var natsPubSub = app.Services.GetRequiredService<NatsPubSub>();

var natsAddress = "localhost:4222";
if(Environment.GetEnvironmentVariable("NATS_ADDRESS") is {} address)
{
    natsAddress = address;
}
natsPubSub.Initialize(natsAddress);

app.Run();
