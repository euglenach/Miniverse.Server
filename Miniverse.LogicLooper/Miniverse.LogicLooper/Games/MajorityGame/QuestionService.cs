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
    private readonly AsyncLock asyncLock = new();
    private readonly Lock sessionLock = new();
    private Ulid roomUlid;
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
    
    public async ValueTask AskQuestion(Ulid playerUlid, string questionText, string[] choices)
    {
        using var ___ = await asyncLock.EnterScope();
        
        if(session is not null)
        {
            logger.LogInformation("既に質問者がいます");
            return;
        }

        var room = await roomInfoProvider.RoomInfoAsyncLazy; 
        roomUlid = room.Ulid;

        // 質問クラス生成
        session = serviceProvider.GetRequiredService<QuestionSession>();
        session.Initialize(playerUlid, questionText, choices);

        // MagicOnionに送る
        var data = new MajorityGameQuestion(playerUlid, questionText, choices);
        await nats.Publish(roomUlid.ToString(), new OnAskedQuestionMsg(data));
    }

    public async ValueTask Select(Ulid playerUlid, int index)
    {
        using var ___ = await asyncLock.EnterScope();
        
        if(session is null)
        {
            logger.LogInformation("質問がありません");
            return;
        }
        
        // 選択の登録
        if(!session.RegisterSelected(playerUlid, index)) return;

        // 選択をMagicOnionに送る(カリング用に質問者のIDも送る)
        await nats.Publish(roomUlid.ToString(), new OnSelectedMsg(session.PlayerUlid, playerUlid, index));
    }

    public async ValueTask ResultOpen(Ulid playerUlid)
    {
        using var ___ = await asyncLock.EnterScope();
        
        if(session is null)
        {
            logger.LogInformation("質問がありません");
            return;
        }

        // 質問者か照合する
        if(session.PlayerUlid != playerUlid) return;
        
        await ResultOpenCore();

        lock(sessionLock)
        {
            session = null;
            elapsed = 0;
        }
    }

    async ValueTask ResultOpenCore()
    {
        if(session is null) return;

        var data = session.CreateResult();
        // 結果をMagicOnionに送る
        await nats.Publish(roomUlid.ToString(), new OnResultMsg(data));
    }
}
