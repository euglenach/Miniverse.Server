﻿using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using R3;
using ZLogger;

namespace Miniverse.CLIClient.StreamingHubs;

public class MatchingHub : IDisposable
{
    private readonly Player player;
    private readonly GrpcChannel channel;
    private readonly IMatchingHub matchingHub;
    private readonly MatchingReceiver receiver;
    
    #region MatchingReceiverEvents
    public Observable<Player> OnJoin => receiver.OnJoin;
    public Observable<MajorityGameRoomInfo> OnJoinSelf => receiver.OnJoinSelf;
    public Observable<Ulid> OnLeave => receiver.OnLeave;

    #endregion
    
    private MatchingHub(Player player, GrpcChannel channel, IMatchingHub matchingHub, MatchingReceiver receiver)
    {
        this.player = player;
        this.channel = channel;
        this.matchingHub = matchingHub;
        this.receiver = receiver;
    }
    
    public static async ValueTask<MatchingHub> CreateAsync(Player player, CancellationToken cancellationToken = default)
    {
        var receiver = new MatchingReceiver();
        var (hub, channel) = await StreamingHubFactory.CreateAsync<IMatchingHub, IMatchingReceiver>(receiver, cancellationToken : cancellationToken);
            
        return new(player, channel, hub, receiver);
    }
    
    public async ValueTask CreateRoomAsync()
    {
        await matchingHub.CreateRoomAsync(player);
    }
        
    public async ValueTask JoinRoomAsync(Ulid roomUlid)
    {
        await matchingHub.JoinRoomAsync(roomUlid, player);
    }
    
    public async void Dispose()
    {
        receiver.Dispose();
        await channel.ShutdownAsync();
        await matchingHub.DisposeAsync();
    }

    private class MatchingReceiver : IMatchingReceiver, IDisposable
    {
        public readonly Subject<Player> OnJoin = new();
        public readonly Subject<MajorityGameRoomInfo> OnJoinSelf = new();
        public readonly Subject<Ulid> OnLeave = new();
            
        void IMatchingReceiver.OnJoin(Player player)
        {
            // LogManager.Global.ZLogDebug($"{nameof(IMatchingReceiver.OnJoin)}");
            OnJoin.OnNext(player);
        }

        void IMatchingReceiver.OnJoinSelf(MajorityGameRoomInfo roomInfo)
        {
            // LogManager.Global.ZLogDebug($"{nameof(IMatchingReceiver.OnJoinSelf)}");
            OnJoinSelf.OnNext(roomInfo);
        }

        void IMatchingReceiver.OnLeave(Ulid playerUlid)
        {
            // LogManager.Global.ZLogDebug($"{nameof(IMatchingReceiver.OnLeave)}");
            OnLeave.OnNext(playerUlid);
        }

        public void Dispose()
        {
            OnJoin.Dispose();
            OnJoinSelf.Dispose();
            OnLeave.Dispose();
        }
    }
}
