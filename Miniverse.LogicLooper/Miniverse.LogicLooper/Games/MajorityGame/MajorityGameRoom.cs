using Cysharp.Threading;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.LogicLooperServer;

public class MajorityGameRoom(ILogger<MajorityGameRoom> logger)
{
    private MajorityGameRoomInfo roomInfo;
    
    public async ValueTask InitializeAsync(MajorityGameRoomInfo roomInfo, CancellationToken token = default)
    {
        this.roomInfo = roomInfo;
    }
    
    public void RoomJoin(Player player)
    {
        logger.ZLogInformation($"{player.Name} is joined. room:{roomInfo.Ulid}");
    }

    public bool Update(in LogicLooperActionContext context)
    {
        logger.ZLogInformation($"Update!");
        return true;
    }
}
