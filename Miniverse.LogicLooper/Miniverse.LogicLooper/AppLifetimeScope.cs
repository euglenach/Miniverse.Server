using Cysharp.Threading;
using Miniverse.LogicLooperServer;
using Miniverse.ServerShared.Nats;
using NATS.Client.Hosting;

namespace Miniverse.LogicLooper;

public class AppLifetimeScope
{
    public static void Configure(IServiceCollection services)
    {
        // フレームワーク
        services.AddNats();
        
        services.AddSingleton<ILogicLooperPool>(_ => new LogicLooperPool(60, Environment.ProcessorCount, RoundRobinLogicLooperPoolBalancer.Instance));
        
        services.AddSingleton<NatsPubSub>();
        services.AddSingleton<NatsReceiver>();
        services.AddSingleton<MatchingReceiver>();
        services.AddSingleton<MajorityGameRoomManager>();
    }
}
