using MiniverseShared.MessagePackObjects;
using ValueTaskSupplement;

namespace Miniverse.LogicLooperServer;

public class RoomInfoProvider : IDisposable
{
    public MajorityGameRoomInfo? RoomInfo{get; private set;}
    private readonly TaskCompletionSource<MajorityGameRoomInfo> roomInfoTaskSource = new();
    public AsyncLazy<MajorityGameRoomInfo> RoomInfoAsyncLazy => ValueTaskEx.Lazy( async () =>
    {
        return RoomInfo = await roomInfoTaskSource.Task;
    });

    public void Register(MajorityGameRoomInfo roomInfo)
    {
        roomInfoTaskSource.TrySetResult(roomInfo);
    }

    public void Dispose()
    {
        roomInfoTaskSource.TrySetCanceled();
    }
}
