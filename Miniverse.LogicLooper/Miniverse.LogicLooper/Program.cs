using Microsoft.AspNetCore.Server.Kestrel.Core;
using Miniverse.LogicLooper;
using Miniverse.LogicLooperServer;
using Miniverse.ServerShared;
using Miniverse.ServerShared.Nats;
using MiniverseShared.Utility;
using ZLogger;

//github.com/Cysharp/LogicLooper/blob/master/samples/LoopHostingApp/Program.cs

MessagePackOptionRegister.Register();
ValueTaskExtensions.RegisterUnhandledExceptionHandler(e =>
{
    LogManager.Logger.ZLogError($"UnhandledException: {e}");
});

var builder = WebApplication.CreateBuilder(args);

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

// static scope logger
LogManager.SetGlobalLoggerFactory(app.Services);

var nats = app.Services.GetRequiredService<NatsPubSub>();
var receiver = app.Services.GetRequiredService<NatsReceiver>();
var matchingReceiver = app.Services.GetRequiredService<MatchingReceiver>();

var natsAddress = "localhost:4222";
if(Environment.GetEnvironmentVariable("NATS_ADDRESS") is {} address)
{
    natsAddress = address;
}
nats.Initialize(natsAddress);

_ = receiver.StartSubscribe();
_ = matchingReceiver.StartSubscribe();

app.Run();
return;

// static IHostBuilder CreateHostBuilder(string[] args) =>
//     Host.CreateDefaultBuilder(args)
//         .ConfigureServices(services =>
//         {
//             services.Configure<HostOptions>(options =>
//             {
//                 options.ShutdownTimeout = TimeSpan.FromSeconds(10);
//             });
//             
//             AppLifetimeScope.Configure(services);
//             MajorityGameLifetimeScope.Configure(services);
//         })
//         .ConfigureWebHostDefaults(webBuilder =>
//         {
//             webBuilder.ConfigureKestrel(options =>
//             {
//                 if(Environment.GetEnvironmentVariable("PORT") is {} port && int.TryParse(port, out var result))
//                 {
//                     options.ListenAnyIP(result);
//                 }
//             });
//         })
//         .ConfigureLogging(logging =>
//         {
//             logging.ClearProviders();
//             logging.AddZLoggerConsole(options =>
//             {
//                 options.UsePlainTextFormatter(formatter =>
//                 {
//                     // 2023-12-19 02:46:14.289 [DBG]......
//                     formatter.SetPrefixFormatter($"{0:local-longdate} [{1:short}] ", (in MessageTemplate template, in LogInfo info) =>
//                                                      template.Format(info.Timestamp, info.LogLevel));
//                 });
//             }).SetMinimumLevel(LogLevel.Debug);
//         });
