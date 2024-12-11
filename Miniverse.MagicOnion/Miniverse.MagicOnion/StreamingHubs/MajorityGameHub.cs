using Grpc.Core;
using MagicOnion.Server.Hubs;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage.MajorityGame;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using R3;
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
    
    public async ValueTask AskQuestion(string questionText, string[] choices)
    {
        await nats.Publish(roomUlid.ToString(), new AskQuestionMsg(playerUlid, questionText, choices));
        logger.LogInformation($"{nameof(AskQuestion)}: Q:{questionText}, C:{string.Join(",", choices)}");
    }

    public async ValueTask Select(int index)
    {
        await nats.Publish(roomUlid.ToString(), new ChoiceSelectMsg(playerUlid, index));
        logger.LogInformation($"{nameof(Select)}: Select:{index}");
    }

    public async ValueTask ResultOpen()
    {
        await nats.Publish(roomUlid.ToString(), new ResultOpenMsg(playerUlid));
        logger.LogInformation($"{nameof(Select)} ResultOpen:{playerUlid}");
    }

    void EventSubscribe()
    {
        // 質問された通知
        nats.Subscribe<OnAskedQuestionMsg>(roomUlid.ToString()).ToObservable()
            .Subscribe(this, static (msg, state) =>
            {
                if(msg.Question is null) return;
                state.BroadcastToSelf(state.room!).OnAskedQuestion(msg.Question);
            }).RegisterTo(cancellation.Token);
        
        // 選択された通知
        nats.Subscribe<OnSelectedMsg>(roomUlid.ToString()).ToObservable()
            .Subscribe(this, static (msg, state) =>
            {
                if(msg.TargetUlid == Ulid.Empty || msg.SelectedPlayerUlid == Ulid.Empty) return;
                
                // 質問者にしか返さない
                if(msg.TargetUlid != state.playerUlid) return;
                state.BroadcastToSelf(state.room!).OnSelected(msg.SelectedPlayerUlid, msg.Index);
            }).RegisterTo(cancellation.Token);
        
        // 結果された通知
        nats.Subscribe<OnResultMsg>(roomUlid.ToString()).ToObservable()
            .Subscribe(this, static (msg, state) =>
            {
                if(msg.Result is null) return;
                state.BroadcastToSelf(state.room!).OnResult(msg.Result);
            }).RegisterTo(cancellation.Token);
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
        EventSubscribe();
        this.room = await Group.AddAsync(playerUlid.ToString());
        BroadcastToSelf(room!).OnConnected();
    }
    
    protected override ValueTask OnDisconnected()
    {
        cancellation.Cancel();
        cancellation.Dispose();
        return ValueTask.CompletedTask;
    }
}
