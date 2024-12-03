using Miniverse.ServerShared.Nats;
using NATS.Client.Hosting;

namespace Miniverse.LogicLooper;

public class AppLifetimeScope
{
    public static void Configure(IServiceCollection services)
    {
        // フレームワーク
        services.AddNats();
        
        services.AddSingleton<NatsPubSub>();
        services.AddSingleton<NatsReceiver>();
    }
}
