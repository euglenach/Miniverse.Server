using Cysharp.Threading;
using Miniverse.ServerShared.Utility;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoomManager(ILogicLooperPool looperPool, ILogger<MajorityGameRoomManager> logger, IServiceScopeFactory scopeFactory)
{
    private readonly Dictionary<Ulid, MajorityGameRoom> gameRooms = new();
    private readonly AsyncLock roomListLock = new();

    public async ValueTask CreateRoomAsync(Ulid roomUlid, Player player, CancellationToken token = default)
    {
        using var __ = await roomListLock.EnterScope();
        if(gameRooms.ContainsKey(roomUlid)) return;
        
        // 部屋ごとのIDスコープの作成
        var scope = scopeFactory.CreateScope();

        // 部屋作成
        var room = scope.ServiceProvider.GetRequiredService<MajorityGameRoom>();
        var roomInfo = new MajorityGameRoomInfo(roomUlid);
        
        await room.InitializeAsync(roomInfo, token);
        gameRooms.Add(roomUlid, room);
        
        // フレームレートの設定
        var option = new LooperActionOptions(60);
        
        // LogicLooperに部屋のUpdateを追加
        _ = looperPool.RegisterActionAsync(room.Update, option);
        
        logger.ZLogInformation($"created MajorityGame room. ulid: {roomUlid}");
    }

    public async ValueTask JoinRoomAsync(Ulid roomUlid, Player player, CancellationToken token = default)
    {
        using var __ = await roomListLock.EnterScope();
        
        if(!gameRooms.TryGetValue(gameRooms.First().Key, out var room)) return;
        
        room.RoomJoin(player);
    }
}
