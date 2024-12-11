using FluentAssertions;
using Miniverse.CLIClient.Utility;
using ZLogger;

namespace Miniverse.CLIClient;

public class MajorityGame
{
    public async ValueTask Run(CancellationToken cancellationToken = default)
    {
        var playerNum = 4;
        var majorityGamePayers = new MajorityGamePayer[playerNum];

        // プレイヤーを作って初期化する
        for(var i = 0; i < majorityGamePayers.Length; i++)
        {
            var player = new MajorityGamePayer(i);
            majorityGamePayers[i] = player;
            await player.InitializeAsync(cancellationToken);
        }

        var host = majorityGamePayers[0];
        var guests = majorityGamePayers[1..];

        // ルーム作成
        await host.Hub.CreateRoomAsync();

        (await Wait.WaitUntil(() => host.RoomInfo is not null, cancellationToken : cancellationToken)).Should().BeTrue();

        var roomUlid = host.RoomInfo.Ulid;

        // ゲストの入室
        foreach(var guest in guests)
        {
            await guest.Hub.JoinRoomAsync(roomUlid);
            (await Wait.WaitUntil(() => guest.RoomInfo is not null, cancellationToken : cancellationToken)).Should().BeTrue();
        }
        
        LogManager.Global.ZLogInformation($"Complete Majority game!");
    }
}
