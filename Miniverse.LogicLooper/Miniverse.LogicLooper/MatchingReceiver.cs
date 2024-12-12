using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using R3;

namespace Miniverse.LogicLooperServer;

public class MatchingReceiver(NatsPubSub nats, MajorityGameRoomManager roomManager) : IDisposable
{
    private readonly CompositeDisposable disposable = new();
    private readonly CancellationTokenSource cancellation = new();
    
    public async ValueTask StartSubscribe(CancellationToken cancellationToken = default)
    {
        // マッチング系のメッセージだけ受信して流す
        nats.Subscribe<CreateRoomMsg>().ToObservable().Subscribe((roomManager, cancellation.Token), static async (msg, x) =>
        {
            var (manager, token) = x;
            await manager.CreateRoomAsync(msg.RoomUlid, msg.Player, token);
        }).AddTo(this.disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
