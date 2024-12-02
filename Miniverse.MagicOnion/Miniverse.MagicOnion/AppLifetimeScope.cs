using Microsoft.Extensions.DependencyInjection;
using Miniverse.ServerShared.Nats;
using NATS.Client.Hosting;

namespace Miniverse.MagicOnion;

public class AppLifetimeScope
{
    public static void Configure(IServiceCollection services)
    {
        // フレームワーク
        services.AddGrpc();
        services.AddMagicOnion();
        services.AddNats();
        
        services.AddSingleton<NatsPubSub>();
    }
}
