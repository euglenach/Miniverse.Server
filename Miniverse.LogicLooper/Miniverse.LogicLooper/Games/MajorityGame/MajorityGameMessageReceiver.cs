using Miniverse.ServerShared.Nats;
using Miniverse.ServerShared.NatsMessage;
using Miniverse.ServerShared.NatsMessage.MajorityGame;
using MiniverseShared.MessagePackObjects;
using R3;

namespace Miniverse.LogicLooperServer;

public class MajorityGameMessageReceiver(NatsPubSub nats, RoomInfoProvider roomInfoProvider, 
                                         MajorityGameRoomManager roomManager, QuestionService questionService) : IDisposable
{
    MajorityGameRoomInfo roomInfo;
    private readonly CompositeDisposable disposable = new();
    
    public async ValueTask StartSubscribe()
    {
        roomInfo =  await roomInfoProvider.RoomInfoAsyncLazy;
        
        // この部屋の入室通知を購読
        nats.Subscribe<JoinRoomMsg>(roomInfo.Ulid.ToString()).ToObservable().Subscribe(roomManager, static async (msg, state) =>
        {
            await state.JoinRoomAsync(msg.RoomUlid, msg.Player);
        }).AddTo(disposable);
        
        // 質問の開始を購読
        nats.Subscribe<AskQuestionMsg>(roomInfo.Ulid.ToString()).ToObservable()
            .Select(questionService, (x, state) => (x, state))
            .SubscribeAwait(async static (x, ct) =>
            {
                var (msg, question) = x;
                await question.AskQuestion(msg.PlayerUlid, msg.QuestionText, msg.Choices);
            }).AddTo(disposable);
        
        // 選択の購読
        nats.Subscribe<ChoiceSelectMsg>(roomInfo.Ulid.ToString()).ToObservable()
            .Select(questionService, (x, state) => (x, state))
            .SubscribeAwait(async static (x, ct) =>
            {
                var (msg, question) = x;
                await question.Select(msg.PlayerUlid, msg.Index);
            }).AddTo(disposable);
        
        // 選択の購読
        nats.Subscribe<ResultOpenMsg>(roomInfo.Ulid.ToString()).ToObservable()
            .Select(questionService, (x, state) => (x, state))
            .SubscribeAwait(async static (x, ct) =>
            {
                var (msg, question) = x;
                await question.ResultOpen(msg.PlayerUlid);
            }).AddTo(disposable);
    }

    public void Dispose()
    {
        
    }
}
