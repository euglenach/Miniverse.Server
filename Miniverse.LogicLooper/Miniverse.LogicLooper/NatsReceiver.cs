using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class NatsReceiver(NatsPubSub nats, ILogger<NatsPubSub> logger)
{
    public async ValueTask StartSubscribe()
    {
        // await foreach (var msg in nats.Subscribe<CreateRoomMsg>())
        // {
        //     logger.ZLogInformation($"Received: {msg}");
        // }
    }
}
