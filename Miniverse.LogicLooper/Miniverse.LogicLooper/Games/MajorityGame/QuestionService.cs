using Cysharp.Threading;
using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage.MajorityGame;
using Miniverse.ServerShared.Utility;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.Utility;
using R3;

namespace Miniverse.LogicLooperServer;

public class QuestionService(IServiceProvider serviceProvider, ILogger<QuestionService> logger, NatsPubSub nats, RoomInfoProvider roomInfoProvider)
{
    private QuestionSession? session;
    MajorityGameRoomInfo? roomInfo;
    private readonly AsyncLock asyncLock = new();
    private readonly Lock sessionLock = new();
    private double elapsed;

    public void Update(in LogicLooperActionContext ctx)
    {
        using var ___ = sessionLock.EnterScope();
        if(session is null) return;
        
        // 前フレームからの経過時間を使って質問が始まってからの経過時間を計測する
        elapsed += ctx.ElapsedTimeFromPreviousFrame.TotalSeconds;
        
        // 開始から10秒立ってたら結果を送る
        if(elapsed >= 10)
        {
            ResultOpenCore().Forget();
        }
    }
    
    public async ValueTask AskQuestion(Ulid playerUlid, string questionText, string[]? choices)
    {
        if(playerUlid == Ulid.Empty || string.IsNullOrEmpty(questionText) || choices is null || choices.Length <= 1)
            return;
        
        using var ___ = await asyncLock.EnterScope();
        
        if(session is not null)
        {
            logger.LogInformation("既に質問者がいます");
            return;
        }

        roomInfo = await roomInfoProvider.RoomInfoAsyncLazy;

        // 質問クラス生成
        session = serviceProvider.GetRequiredService<QuestionSession>();
        session.Initialize(playerUlid, questionText, choices);

        // MagicOnionに送る
        var data = new MajorityGameQuestion(playerUlid, questionText, choices);
        await nats.Publish(roomInfo.Ulid.ToString(), new OnAskedQuestionMsg(data));
    }

    public async ValueTask Select(Ulid playerUlid, int index)
    {
        if(playerUlid == Ulid.Empty || roomInfo is null) return;
        using var ___ = await asyncLock.EnterScope();
        
        if(session is null)
        {
            logger.LogInformation("質問がありません");
            return;
        }
        
        // 選択の登録
        if(!session.RegisterSelected(playerUlid, index)) return;

        // 選択をMagicOnionに送る(カリング用に質問者のIDも送る)
        await nats.Publish(roomInfo.Ulid.ToString(), new OnSelectedMsg(session.PlayerUlid, playerUlid, index));
    }

    public async ValueTask ResultOpen(Ulid playerUlid)
    {
        if(playerUlid == Ulid.Empty) return;
        using var ___ = await asyncLock.EnterScope();
        
        if(session is null)
        {
            logger.LogInformation("質問がありません");
            return;
        }

        // 質問者か照合する
        if(session.PlayerUlid != playerUlid) return;
        
        await ResultOpenCore();
    }

    async ValueTask ResultOpenCore()
    {
        if(session is null || roomInfo is null) return;

        var data = session.CreateResult();
        session = null;
        elapsed = 0;
        
        // 結果をMagicOnionに送る
        await nats.Publish(roomInfo.Ulid.ToString(), new OnResultMsg(data));
    }
}
