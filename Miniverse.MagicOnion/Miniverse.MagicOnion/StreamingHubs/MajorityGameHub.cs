using Grpc.Core;
using MagicOnion.Server.Hubs;
using Miniverse.ServerShared.Nats;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using ZLogger;

namespace Miniverse.MagicOnion.StreamingHubs;

public class MajorityGameHub(ILogger<MatchingHub> logger, NatsPubSub nats) : StreamingHubBase<IMajorityGameHub, IMajorityGameReceiver>, IMajorityGameHub
{
    private IGroup? room;
    private Ulid playerUlid;
    private Ulid roomUlid;
    private NatsPubSub nats{get;} = nats;
    private ILogger<MatchingHub> logger{get;} = logger;
    private readonly CancellationTokenSource cancellation = new();

    public async ValueTask AskQuestion(string questionText)
    {
        
    }

    public async ValueTask Select(int index)
    {
        
    }

    public async ValueTask ResultAsync()
    {
        
    }
    
    protected override ValueTask OnConnecting()
    {
        var playerUlidHeader = Context.CallContext.RequestHeaders.GetValue("player_ulid");
        var roomUlidHeader = Context.CallContext.RequestHeaders.GetValue("room_ulid");
        if(!Ulid.TryParse(playerUlidHeader, out playerUlid)) logger.ZLogError($"player_ulid: {playerUlidHeader} is not a valid Ulid");
        if(!Ulid.TryParse(roomUlidHeader, out roomUlid)) logger.ZLogError($"room_ulid: {roomUlidHeader} is not a valid Ulid");
        return ValueTask.CompletedTask;
    }

    protected override async ValueTask OnConnected()
    {
        this.room = await Group.AddAsync(playerUlid.ToString());
    }
    
    protected override ValueTask OnDisconnected()
    {
        cancellation.Cancel();
        cancellation.Dispose();
        return ValueTask.CompletedTask;
    }
}
