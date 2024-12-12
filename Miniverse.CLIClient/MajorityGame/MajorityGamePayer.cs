using Cysharp.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Miniverse.CLIClient.StreamingHubs;
using MiniverseShared.MessagePackObjects;
using R3;
using ZLogger;

namespace Miniverse.CLIClient;

public class MajorityGamePayer : IDisposable
{
    public Player Player{get;}

    public MatchingHub MatchingHub{get; private set;}
    public MajorityGameHub MajorityGameHub{get; private set;}
    public MajorityGameRoomInfo? RoomInfo{get;private set;}
    public MajorityGameResult? Result{get;private set;}
    private readonly CompositeDisposable disposable = new();
    private readonly ILogger<MajorityGamePayer> logger;
    public bool IsConnectedMajorityGame { get; private set; }
    
    public MajorityGamePayer(int index)
    {
        Player = new(Ulid.NewUlid(), $"CliClient{(index == 0? "Host" : "Guest")}Player{index}");
        logger = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(index == 0 ? LogLevel.Debug : LogLevel.None);
            logging.AddZLoggerConsole(options => options.UsePlainTextFormatter(formatter => 
                formatter.SetPrefixFormatter($"{0:timeonly} [{1:short}] [{2:short}] ",
                                             (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel, Player.Ulid.ToString()))));
        }).CreateLogger<MajorityGamePayer>();
    }

    /// <summary>
    /// マッチング用の接続
    /// </summary>
    public async ValueTask ConnectMatchingAsync(CancellationToken cancellationToken = default)
    {
        MatchingHub = await MatchingHub.CreateAsync(Player);
        MatchingEventSubscribe();
    }

    /// <summary>
    /// マジョリティゲーム用の接続
    /// </summary>
    public async ValueTask ConnectGameAsync(CancellationToken cancellationToken = default)
    {
        MajorityGameHub = await MajorityGameHub.CreateAsync(Player, RoomInfo.Ulid, cancellationToken);
        MajorityGameEventSubscribe();
    }
    
    void MatchingEventSubscribe()
    {
        MatchingHub.OnJoinSelf.Subscribe(this, static (roomInfo, state) =>
        {
            state.RoomInfo = roomInfo;
            state.logger.ZLogInformation($"{nameof(MatchingHub.OnJoinSelf)}: room:{roomInfo.Ulid} me:{state.Player.Name}");
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
        // OnConnectedのイベント購読
        MajorityGameHub.OnConnected.Subscribe(this, (_, state) =>
        {
            state.IsConnectedMajorityGame = true;
        }).AddTo(disposable);
        
        // OnAskedQuestionのイベント購読
        MajorityGameHub.OnAskedQuestion.Subscribe(this, (question, state) =>
        {
            Result = null;
            var asker = state.RoomInfo!.Players.FirstOrDefault(p => p.Ulid == question.AskedPlayerUlid);
            asker.Should().NotBeNull();

            // クライアントにもデータを反映する
            state.RoomInfo.Question = question;
            state.logger.ZLogInformation($"{asker!.Name}が質問を開始しました。質問文:{question.QuestionText} 選択肢:{string.Join(",", question.Choices)}");
        }).AddTo(disposable);
        
        // OnAskedQuestionのイベント購読
        MajorityGameHub.OnSelected.Subscribe(this, (args, state) =>
        {
            // (質問者だけに飛んでくるのでクライアントが判断しなくていい)
            var answer = state.RoomInfo!.Players.FirstOrDefault(p => p.Ulid == args.AnswerPlayerUlid);
            state.logger.ZLogInformation($"{answer.Name}が{args.Index}を選択しています... me:{state.Player.Name}");
        }).AddTo(disposable);
        
        // OnResultのイベント購読
        MajorityGameHub.OnResult.Subscribe(this, (result, state) =>
        {
            state.logger.ZLogInformation($"結果発表！！");

            float total = result.NumTable.Sum();

            // stringBuilder.Append($"\n【結果】");
            logger.ZLogInformation($"【結果】");
            foreach(var (count, choice) in result.NumTable.Zip(RoomInfo.Question.Choices, (count, choice) => (count, choice))
                                            .OrderByDescending(x => x.count))
            {
                // stringBuilder.Append($"\n{choice}:{count}人...({(int)(count / total * 100)}%)");
                logger.ZLogInformation($"{choice}:{count}人...({(int)(count / total * 100)}%)");
            }
            
            Result = result;
            var str = string.Join("と", result.Majorities.Select(m => state.RoomInfo!.Players.Find(x => x.Ulid == m).Name));
            state.logger.ZLogInformation($"{str}がマジョリティでした！");
            if(RoomInfo.Question.AskedPlayerUlid == Player.Ulid) return;
            state.logger.ZLogInformation($"あなたは{(result.Majorities.Contains(state.Player.Ulid)? "勝利！" : "マイノリティ！！！")} me:{state.Player.Name}");
        }).AddTo(disposable);
    }

    public void Dispose()
    {
        disposable.Dispose();
        MatchingHub.Dispose();
        MajorityGameHub?.Dispose();
    }
}
