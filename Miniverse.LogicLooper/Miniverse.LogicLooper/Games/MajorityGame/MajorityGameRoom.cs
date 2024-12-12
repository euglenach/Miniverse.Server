using Cysharp.Threading;
using Miniverse.LogicLooper.LooperTasks;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoom(ILogger<MajorityGameRoom> logger, NatsPubSub nats, RoomInfoProvider roomInfoProvider, 
                              MajorityGameMessageReceiver messageReceiver, QuestionService questionService)
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

    public async ValueTask RoomLeave(Ulid playerUlid)
    {
        // 退室通知のあったプレイヤーをリストから削除する
        if(roomInfo is null) return;
        if(roomInfo.Players.RemoveAll(p => p.Ulid == playerUlid) < 1) return;
        
        logger.ZLogInformation($"{playerUlid} is leaved.");
        await nats.Publish(roomInfo.Ulid.ToString(), new OnLeaveRoomMsg(playerUlid));
    }

    public bool Update(in LogicLooperActionContext context)
    {
        // ルームから全員が退室していたらUpdateを止める
        if(roomInfo is null) return true;
        if(roomInfo.Players.Count == 0) return false;
        questionService.Update(context);
        // logger.ZLogInformation($"Update!!!");
        return true;
    }
}
