using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using R3;
using ZLogger;

namespace Miniverse.MagicOnion.StreamingHubs;

public class MatchingHub(ILogger<MatchingHub> logger, NatsPubSub nats) : StreamingHubBase<IMatchingHub, IMatchingReceiver>, IMatchingHub
{
    private IGroup? room;
    private Player? player;
    private Ulid roomUlid;
    private NatsPubSub nats{get;} = nats;
    private ILogger<MatchingHub> logger{get;} = logger;
    private readonly CancellationTokenSource cancellation = new();

    public async ValueTask CreateRoomAsync(Player player)
    {
        if(room is not null || this.player is not null) return;
        this.roomUlid = Ulid.NewUlid();
        this.player = player;
        this.room = await Group.AddAsync(player.Ulid.ToString());
        logger.ZLogInformation($"Creating matching...  Player:{player.Ulid}: roomUlid:{roomUlid}");
        
        // natsで部屋を生成したことをLogicLooperに投げたい
        await nats.Publish(new CreateRoomMsg(roomUlid, player));
        SubscribeFromNatsMessage(roomUlid);
    }

    public async ValueTask JoinRoomAsync(Ulid roomUlid, Player player)
    {
        if(room is not null || this.player is not null) return;
        this.roomUlid = roomUlid;
        this.player = player;
        this.room = await Group.AddAsync(player.Ulid.ToString());
        
        logger.ZLogInformation($"Joining matching hub... Player:{player.Ulid}: roomUlid:{roomUlid}");
        
        // natsで部屋を生成したことをLogicLooperに投げたい
        await nats.Publish(roomUlid.ToString(), new JoinRoomMsg(roomUlid, player));
        SubscribeFromNatsMessage(roomUlid);
    }

    private void SubscribeFromNatsMessage(Ulid roomUlid)
    {
        // BroadcastToSelfでOnJoinをクライアントに流す
        nats.Subscribe<OnJoinSelfMsg>(roomUlid.ToString()).ToObservable()
            .Subscribe(this, (msg, state) =>
            {
                if(msg.Player is null || msg.RoomInfo is null) return;
                if(msg.Player.Ulid == state.player!.Ulid) return;
                state.BroadcastToSelf(state.room!).OnJoin(msg.Player);
                state.logger.ZLogInformation($"Joined room:{state.roomUlid} player:{msg.Player}");
            }).RegisterTo(cancellation.Token);
        
        // BroadcastToSelfでOnJoinSelfMsgをクライアントに流す
        nats.Subscribe<OnJoinSelfMsg>(roomUlid.ToString()).ToObservable()
                  .Subscribe(this, (msg, state) =>
                  {
                      if(msg.Player is null || msg.RoomInfo is null) return;
                      if(msg.Player.Ulid != state.player!.Ulid) return;
                      state.BroadcastToSelf(state.room!).OnJoinSelf(msg.RoomInfo);
                      state.logger.ZLogInformation($"Joined room:{state.roomUlid} player:{msg.Player}");
                  }).RegisterTo(cancellation.Token);
        
        // BroadcastToSelfでOnJoinをクライアントに流す
        nats.Subscribe<OnLeaveRoomMsg>(roomUlid.ToString()).ToObservable()
            .Subscribe(this, (msg, state) =>
            {
                state.BroadcastToSelf(state.room!).OnLeave(msg.PlayerUlid);
                state.logger.ZLogInformation($"Leaved room:{state.roomUlid} player:{msg.PlayerUlid}");
            }).RegisterTo(cancellation.Token);
    }

    protected override async ValueTask OnDisconnected()
    {
        // LogicLooperに切断を通知
        if(player is not null) await nats.Publish(roomUlid.ToString(), new DisconnectMsg(player.Ulid));
        cancellation.Cancel();
        cancellation.Dispose();
    }
}

