using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using Miniverse.ServerShared.Nats;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using ZLogger;

namespace Miniverse.MagicOnion.StreamingHubs;

public class MatchingHub(ILogger<MatchingHub> logger, NatsPubSub nats) : StreamingHubBase<IMatchingHub, IMatchingReceiver>, IMatchingHub
{
    private Player player;
    
    public async ValueTask JoinAsync(Player player, Ulid roomUlid)
    {
        this.player = player;
        logger.ZLogInformation($"Joining matching hub... RoomUlid:{roomUlid} Player:{player.Ulid}");
        await nats.Publish("LL", "LLLL");
    }
}
