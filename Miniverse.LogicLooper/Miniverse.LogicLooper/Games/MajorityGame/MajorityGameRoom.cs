using Cysharp.Threading;
using Miniverse.LogicLooper.LooperTasks;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoom(ILogger<MajorityGameRoom> logger, NatsPubSub nats, RoomInfoProvider roomInfoProvider, 
                              MajorityGameMessageReceiver messageReceiver, QuestionService questionService, LooperHelper looperHelper, LooperTask looperTask)
{
    private MajorityGameRoomInfo? roomInfo;
    
    public async ValueTask InitializeAsync(MajorityGameRoomInfo roomInfo, CancellationToken token = default)
    {
        if(this.roomInfo is not null) return;
        logger.ZLogInformation($"created MajorityGame room. ulid: {roomInfo.Ulid}");
        
        // いろいろ初期化
        this.roomInfo = roomInfo;
        roomInfoProvider.Register(roomInfo);
        await messageReceiver.StartSubscribe();
        
        logger.ZLogInformation($"MajorityGameRoom initialization complete. ulid: {roomInfo.Ulid}");
        
        // MagicOnionにOnJoin通知する
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinSelfMsg(roomInfo, roomInfo.Players[0]));
    }
    
    public async ValueTask RoomJoin(Player player)
    {
        if(player is null || roomInfo is null || roomInfo.Players.Contains(player, EqualityComparer<Player>.Create((a, b) => (a is null && b is null) || (a is not null && b is not null && a.Ulid == b.Ulid))))
            return;
        logger.ZLogInformation($"{player.Name} is joined. room:{roomInfo.Ulid}");
        roomInfo.Players.Add(player);
        
        // MagicOnionにOnJoin通知する
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinSelfMsg(roomInfo, player));
        await nats.Publish(roomInfo.Ulid.ToString(), new OnJoinMsg(roomInfo.Ulid, player));
    }

    public bool Update(in LogicLooperActionContext context)
    {
        looperHelper.Update(context);
        questionService.Update(context);
        return true;
    }
}
