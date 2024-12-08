using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using ZLogger;

namespace Miniverse.MagicOnion.StreamingHubs;

public class MatchingHub(ILogger<MatchingHub> logger, NatsPubSub nats) : StreamingHubBase<IMatchingHub, IMatchingReceiver>, IMatchingHub
{
    private IGroup? room;
    private Player? player;
    private Ulid roomUlid;

    public async ValueTask CreateRoomAsync(Player player)
    {
        if(room is not null || this.player is not null) return;
        this.roomUlid = Ulid.NewUlid();
        this.player = player;
        this.room = await Group.AddAsync(player.Ulid.ToString());
        logger.ZLogInformation($"Joining matching hub... Player:{player.Ulid}: roomUlid:{roomUlid}");
        
        // natsで部屋を生成したことをLogicLooperに投げたい
        await nats.Publish(new CreateRoomMsg(roomUlid, player));
    }

    public async ValueTask JoinRoomAsync(Ulid roomUlid, Player player)
    {
        if(room is not null || this.player is not null) return;
        this.roomUlid = roomUlid;
        this.player = player;
        this.room = await Group.AddAsync(player.Ulid.ToString());
        
        logger.ZLogInformation($"Joining matching hub... Player:{player.Ulid}: roomUlid:{roomUlid}");
        
        // natsで部屋を生成したことをLogicLooperに投げたい
        await nats.Publish(new JoinRoomMsg(roomUlid, player));
    }
}

