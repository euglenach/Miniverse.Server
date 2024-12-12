using FluentAssertions;
using Miniverse.CLIClient.Utility;
using MiniverseShared.Utility;
using R3;
using ValueTaskSupplement;
using ZLogger;

namespace Miniverse.CLIClient;

public class MajorityGame
{
    public async ValueTask Run(CancellationToken cancellationToken = default)
    {
        using var disposables = new CompositeDisposable();
        
        LogManager.Global.ZLogInformation($"Start MajorityGame CLI...");
        
        var playerNum = 4;
        var majorityGamePayers = new MajorityGamePayer[playerNum];

        // プレイヤーを作って初期化する
        for(var i = 0; i < majorityGamePayers.Length; i++)
        {
            var player = new MajorityGamePayer(i);
            disposables.Add(player);
            majorityGamePayers[i] = player;
            // 初期化。MagicOnionの接続をしたり
            await player.ConnectMatchingAsync(cancellationToken);
        }

        var host = majorityGamePayers[0];
        var guests = majorityGamePayers[1..];

        // ルーム作成
        await host.MatchingHub.CreateRoomAsync();

        (await Wait.Until(() => host.RoomInfo is not null, cancellationToken : cancellationToken)).Should().BeTrue();

        var roomUlid = host.RoomInfo.Ulid;

        // ゲストの入室
        foreach(var guest in guests)
        {
            await guest.MatchingHub.JoinRoomAsync(roomUlid);
            (await Wait.Until(() => guest.RoomInfo is not null, cancellationToken : cancellationToken)).Should().BeTrue();
        }

        // 人数チェック
        foreach(var player in majorityGamePayers)
        {
            player.RoomInfo.Should().NotBeNull();
            player.RoomInfo!.Players.Count.Should().Be(majorityGamePayers.Length);
        }
        
        LogManager.Global.ZLogInformation($"Complete Matching!!");
        
        // 全員の接続を待つ
        await ValueTaskEx.WhenAll(
            majorityGamePayers.Select(x => x.ConnectGameAsync(cancellationToken))
            );
        (await Wait.Until(() => majorityGamePayers.All(p => p.IsConnectedMajorityGame), cancellationToken : cancellationToken)).Should().BeTrue();
        
        // ここからマジョリティゲーム。プログラムで直接シナリオを書く
        
        // 質問開始
        var choices = new[]{"犬", "猫", "うさぎ", "虚無"};
        await host.MajorityGameHub.AskQuestion("飼うなら？", choices);
        
        // 全員に質問が受信されるか確認
        (await Wait.Until(() => majorityGamePayers.All(p => p.RoomInfo!.Question is not null), cancellationToken : cancellationToken)).Should().BeTrue();

        foreach(var guest in guests)
        {
            // ランダムで1つ選択させる
            guest.MajorityGameHub.Select(Random.Shared.Next(choices.Length)).Forget();
            
            // ホストだけに届く通知を待機
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(2000);
            await host.MajorityGameHub.OnSelected.Where(guest, static (x, g) => x.AnswerPlayerUlid == g.Player.Ulid).FirstAsync(cancellationToken : cts.Token);
        }

        // ちょっと待ってから結果発表
        await Task.Delay(1000, cancellationToken);
        host.MajorityGameHub.ResultOpen().Forget();
        
        // リザルトが来るのを待つ
        (await Wait.Until(() => majorityGamePayers.All(p => p.Result is not null), cancellationToken : cancellationToken)).Should().BeTrue();
        
        LogManager.Global.ZLogInformation($"Complete MajorityGame!!");
    }
}
