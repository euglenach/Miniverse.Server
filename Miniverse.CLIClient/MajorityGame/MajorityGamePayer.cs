using FluentAssertions;
using Microsoft.Extensions.Logging;
using Miniverse.CLIClient.StreamingHubs;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.CLIClient;

public class MajorityGamePayer : IDisposable
{
    private readonly Player player;
    public MatchingHub Hub{get; private set;}
    public MajorityGameRoomInfo? RoomInfo{get;private set;}
    private readonly CompositeDisposable disposable = new();
    private ILogger<MajorityGamePayer> logger;
    
    public MajorityGamePayer(int index)
    {
        player = new(Ulid.NewUlid(), $"CliClient{(index == 0? "Host" : "Guest")}Player{index}");
        logger = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddZLoggerConsole(options => options.UsePlainTextFormatter(formatter => 
                formatter.SetPrefixFormatter($"{0:local-longdate} [{1:short}] [{2:short}] ",
                                             (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel, player.Ulid.ToString()))));
        }).CreateLogger<MajorityGamePayer>();
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        Hub = await MatchingHub.CreateAsync(player);
        EventSubscribe();
    }

    void EventSubscribe()
    {
        Hub.OnJoinSelf.Subscribe(this, static (roomInfo, state) =>
        {
            state.RoomInfo = roomInfo;
            state.logger.ZLogDebug($"{nameof(Hub.OnJoinSelf)}: room:{roomInfo.Ulid} me:{state.player.Name}");
        }).AddTo(disposable);
        
        Hub.OnJoin.Subscribe(this, static (player, state) =>
           {
               if(state.RoomInfo is null) return;
               state.RoomInfo.Players.Add(player);
               state.logger.ZLogDebug($"{nameof(Hub.OnJoin)}: {player.Name}");
           })
           .AddTo(disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
