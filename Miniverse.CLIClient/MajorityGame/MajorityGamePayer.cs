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

    /// <summary>
    /// マッチング用の接続
    /// </summary>
    public async ValueTask ConnectMatchingAsync(CancellationToken cancellationToken = default)
    {
        MatchingHub = await MatchingHub.CreateAsync(player);
        MatchingEventSubscribe();
    }

    /// <summary>
    /// マジョリティゲーム用の接続
    /// </summary>
    public async ValueTask ConnectGameAsync(CancellationToken cancellationToken = default)
    {
        MajorityGameHub = await MajorityGameHub.CreateAsync(player, RoomInfo.Ulid, cancellationToken);
        MajorityGameEventSubscribe();
    }
    
    void MatchingEventSubscribe()
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

    void MajorityGameEventSubscribe()
    {
        // OnAskedQuestionのイベント購読
        MajorityGameHub.OnAskedQuestion.Subscribe(this, (x, state) =>
        {
            var asker = state.RoomInfo!.Players.FirstOrDefault(p => p.Ulid == x.AskedPlayerUlid);
            asker.Should().NotBeNull();

            // クライアントにもデータを反映する
            state.RoomInfo.Question = x;
            state.logger.ZLogInformation($"{asker!.Name}が質問を開始しました。質問文:{x.QuestionText} 選択肢:{string.Join(",", x.Choices)}");
        }).AddTo(disposable);
        
        // OnAskedQuestionのイベント購読
        MajorityGameHub.OnSelected.Subscribe(this, (x, state) =>
        {
            // (質問者だけに飛んでくるのでクライアントが判断しなくていい)
            var answer = state.RoomInfo!.Players.FirstOrDefault(p => p.Ulid == x.AnswerPlayerUlid);
            state.logger.ZLogInformation($"{answer.Name}が{x.Index}を選択しています... me:{state.player.Name}");
        }).AddTo(disposable);
        
        // OnResultのイベント購読
        MajorityGameHub.OnResult.Subscribe(this, (x, state) =>
        {
            // (質問者だけに飛んでくるのでクライアントが判断しなくていい)
            var str = string.Join("と", x.Majorities.Select(m => state.RoomInfo!.Players.Find(x => x.Ulid == m)));
            state.logger.ZLogInformation($"{str}がマジョリティでした！");
            state.logger.ZLogInformation($"あなたは{(x.Majorities.Contains(state.player.Ulid)? "敗北..." : "勝利！")}");
        }).AddTo(disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
