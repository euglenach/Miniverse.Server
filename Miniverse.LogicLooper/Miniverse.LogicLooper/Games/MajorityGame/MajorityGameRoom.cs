using Cysharp.Threading;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoom(ILogger<MajorityGameRoom> logger, NatsPubSub nats)
{
    private MajorityGameRoomInfo roomInfo;
    
    public async ValueTask InitializeAsync(MajorityGameRoomInfo roomInfo, CancellationToken token = default)
    {
        logger.ZLogInformation($"created MajorityGame room. ulid: {roomInfo.Ulid}");
        this.roomInfo = roomInfo;
        
        // MagicOnionにOnJoin通知する
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinSelfMsg(roomInfo, roomInfo.Players[0]));
    }
    
    public async ValueTask RoomJoin(Player player)
    {
        logger.ZLogInformation($"{player.Name} is joined. room:{roomInfo.Ulid}");
        roomInfo.Players.Add(player);
        
        // MagicOnionにOnJoin通知する
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinSelfMsg(roomInfo, player));
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinMsg(roomInfo.Ulid, player));
    }

    public bool Update(in LogicLooperActionContext context)
    {
        logger.ZLogInformation($"Update!");
        return true;
    }
}
