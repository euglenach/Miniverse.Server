using FluentAssertions;
using Miniverse.CLIClient.StreamingHubs;
using MiniverseShared.MessagePackObjects;
using R3;

namespace Miniverse.CLIClient;

public class MajorityGamePayer : IDisposable
{
    private readonly Player player;
    public MatchingHub Hub{get; private set;}
    public MajorityGameRoomInfo? RoomInfo{get;private set;}
    private readonly CompositeDisposable disposable = new();
    
    public MajorityGamePayer(int index)
    {
        player = new(Ulid.NewUlid(), $"CliClient{(index == 0? "Host" : "Guest")}Player{index}");
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        Hub = await MatchingHub.CreateAsync(player);
        EventSubscribe();
    }

    void EventSubscribe()
    {
        Hub.OnJoinSelf.Subscribe(this, static (roomInfo, state) => state.RoomInfo = roomInfo).AddTo(disposable);
        
        Hub.OnJoin.Subscribe(this, static (player, state) =>
           {
               state.RoomInfo.Should().NotBeNull();
               state.RoomInfo.Players.Add(player);
           })
           .AddTo(disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
