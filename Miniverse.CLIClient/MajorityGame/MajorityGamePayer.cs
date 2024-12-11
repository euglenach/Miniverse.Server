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
    public MatchingHub MatchingHub{get; private set;}
    public MajorityGameHub MajorityGameHub{get; private set;}
    public MajorityGameRoomInfo? RoomInfo{get;private set;}
    private readonly CompositeDisposable disposable = new();
    private readonly ILogger<MajorityGamePayer> logger;
    
    public MajorityGamePayer(int index)
    {
        player = new(Ulid.NewUlid(), $"CliClient{(index == 0? "Host" : "Guest")}Player{index}");
        logger = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddZLoggerConsole(options => options.UsePlainTextFormatter(formatter => 
                formatter.SetPrefixFormatter($"{0:timeonly} [{1:short}] [{2:short}] ",
                                             (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel, player.Ulid.ToString()))));
        }).CreateLogger<MajorityGamePayer>();
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        MatchingHub = await MatchingHub.CreateAsync(player);
        MajorityGameHub = await MajorityGameHub.CreateAsync(player);
        EventSubscribe();
    }

    void EventSubscribe()
    {
        MatchingHub.OnJoinSelf.Subscribe(this, static (roomInfo, state) =>
        {
            state.RoomInfo = roomInfo;
            state.logger.ZLogInformation($"{nameof(MatchingHub.OnJoinSelf)}: room:{roomInfo.Ulid} me:{state.player.Name}");
        }).AddTo(disposable);
        
        MatchingHub.OnJoin.Subscribe(this, static (player, state) =>
           {
               if(state.RoomInfo is null) return;
               state.RoomInfo.Players.Add(player);
               state.logger.ZLogInformation($"{nameof(MatchingHub.OnJoin)}: {player.Name}");
           })
           .AddTo(disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
