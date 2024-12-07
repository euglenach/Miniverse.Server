using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using ZLogger;

namespace Miniverse.LogicLooper;

public class NatsReceiver(NatsPubSub nats, ILogger<NatsPubSub> logger)
{
    public async ValueTask StartSubscribe()
    {
        // Subscribe on one terminal
        // await foreach (var msg in nats.Subscribe<CreateRoomMsg>())
        // {
        //     logger.ZLogInformation($"Received: {msg}");
        // }
        await foreach (var msg in nats.Subscribe<CreateRoomMsg>())
        {
            logger.ZLogInformation($"Received: {msg}");
        }
    }
}
