using Miniverse.LogicLooper;
using Miniverse.ServerShared;
using Miniverse.ServerShared.Nats;
using ZLogger;

//github.com/Cysharp/LogicLooper/blob/master/samples/LoopHostingApp/Program.cs

MessagePackOptionRegister.Register();

var build = CreateHostBuilder(args).Build();

var nats = build.Services.GetRequiredService<NatsPubSub>();
var receiver = build.Services.GetRequiredService<NatsReceiver>();

LogManager.SetGlobalLoggerFactory(build.Services);

nats.Initialize("localhost:4222");

_ = receiver.StartSubscribe();

build.Run();
return;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.Configure<HostOptions>(options =>
            {
                options.ShutdownTimeout = TimeSpan.FromSeconds(10);
            });
            
            AppLifetimeScope.Configure(services);
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddZLoggerConsole(options =>
            {
                options.UsePlainTextFormatter(formatter =>
                {
                    // 2023-12-19 02:46:14.289 [DBG]......
                    formatter.SetPrefixFormatter($"{0:local-longdate} [{1:short}] ", (in MessageTemplate template, in LogInfo info) =>
                                                     template.Format(info.Timestamp, info.LogLevel));
                });
            }).SetMinimumLevel(LogLevel.Debug);
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
