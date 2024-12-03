using Miniverse.ServerShared.Nats;
using ZLogger;

namespace Miniverse.LogicLooper;

public class NatsReceiver(NatsPubSub nats, ILogger<NatsPubSub> logger)
{
    public async ValueTask StartSubscribe()
    {
        // Subscribe on one terminal
        await foreach (var msg in nats.Subscribe<string>("LL"))
        {
            logger.ZLogInformation($"Received: {msg.Data}");
        }
    }
}
