using FluentAssertions;
using Miniverse.CLIClient.Utility;
using ValueTaskSupplement;
using ZLogger;

namespace Miniverse.CLIClient;

public class MajorityGame
{
    public async ValueTask Run(CancellationToken cancellationToken = default)
    {
        LogManager.Global.ZLogInformation($"Start MajorityGame CLI...");
        
        var playerNum = 4;
        var majorityGamePayers = new MajorityGamePayer[playerNum];

        // プレイヤーを作って初期化する
        for(var i = 0; i < majorityGamePayers.Length; i++)
        {
            var player = new MajorityGamePayer(i);
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
        
        
    }
}
