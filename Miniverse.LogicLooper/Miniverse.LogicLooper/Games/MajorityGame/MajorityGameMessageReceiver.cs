using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using R3;

namespace Miniverse.LogicLooperServer;

public class MajorityGameMessageReceiver(NatsPubSub nats, RoomInfoProvider roomInfoProvider, MajorityGameRoomManager roomManager) : IDisposable
{
    MajorityGameRoomInfo roomInfo;
    private readonly CompositeDisposable disposable = new();
    
    public async ValueTask StartSubscribe()
    {
        roomInfo =  await roomInfoProvider.RoomInfoAsyncLazy;
        
        // この部屋の入室通知を購読
        nats.Subscribe<JoinRoomMsg>(roomInfo.Ulid.ToString()).ToObservable().Subscribe(roomManager, static async (msg, state) =>
        {
            await state.JoinRoomAsync(msg.RoomUlid, msg.Player);
        }).AddTo(this.disposable);
    }

    public void Dispose()
    {
        
    }
}
