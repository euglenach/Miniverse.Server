using Cysharp.Threading;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using Miniverse.ServerShared.Utility;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoomManager(ILogicLooperPool looperPool, ILogger<MajorityGameRoomManager> logger, IServiceScopeFactory scopeFactory, NatsPubSub nats) : IDisposable
{
    private readonly Dictionary<Ulid, MajorityGameRoom> gameRooms = new();
    private readonly AsyncLock roomListLock = new();
    private readonly CompositeDisposable disposable = new();

    public async ValueTask CreateRoomAsync(Ulid roomUlid, Player player, CancellationToken token = default)
    {
        var room = default(MajorityGameRoom);
        var scope = default(IServiceScope);
        
        using(await roomListLock.EnterScope())
        {
            if(gameRooms.ContainsKey(roomUlid)) return;
        
            // 部屋ごとのIDスコープの作成
            scope = scopeFactory.CreateScope();

            // 部屋作成
            room = scope.ServiceProvider.GetRequiredService<MajorityGameRoom>();
            gameRooms.Add(roomUlid, room);
        }
        
        var roomInfo = new MajorityGameRoomInfo(roomUlid, [player], null);
        await room.InitializeAsync(roomInfo, token);
        
        // フレームレートの設定
        var option = new LooperActionOptions(60);
        
        // LogicLooperに部屋のUpdateを追加
        await looperPool.RegisterActionAsync(room.Update, option);
        
        // Updateが終わったら部屋から消す
        using(await roomListLock.EnterScope())
        {
            gameRooms.Remove(roomUlid);
            scope.Dispose();
        }
    }

    public async ValueTask JoinRoomAsync(Ulid roomUlid, Player player, CancellationToken token = default)
    {
        using var __ = await roomListLock.EnterScope();
        
        if(!gameRooms.TryGetValue(roomUlid, out var room)) return;
        
        await room.RoomJoin(player);
    }

    public async ValueTask LeaveRoomAsync(Ulid roomUlid, Ulid playerUlid, CancellationToken token = default)
    {
        using var __ = await roomListLock.EnterScope();
        
        if(!gameRooms.TryGetValue(roomUlid, out var room)) return;
        
        await room.RoomLeave(playerUlid);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
