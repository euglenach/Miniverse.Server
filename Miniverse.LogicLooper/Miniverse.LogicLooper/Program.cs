using Miniverse.LogicLooper;
using Miniverse.ServerShared.Nats;
using ZLogger;

//github.com/Cysharp/LogicLooper/blob/master/samples/LoopHostingApp/Program.cs


var build = CreateHostBuilder(args).Build();

var nats = build.Services.GetRequiredService<NatsPubSub>();
var receiver = build.Services.GetRequiredService<NatsReceiver>();
nats.Initialize("demo.nats.io");

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
            logging.AddZLoggerConsole().SetMinimumLevel(LogLevel.Debug);
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
