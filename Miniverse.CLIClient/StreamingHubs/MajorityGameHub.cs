using Grpc.Net.Client;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using R3;

namespace Miniverse.CLIClient.StreamingHubs;

public class MajorityGameHub : IDisposable
{
    private readonly Player player;
    private readonly GrpcChannel channel;
    private readonly IMajorityGameHub hub;
    private readonly MajorityGameReceiver receiver;

    private MajorityGameHub(Player player, GrpcChannel channel, IMajorityGameHub hub, MajorityGameReceiver receiver)
    {
        this.player = player;
        this.channel = channel;
        this.hub = hub;
        this.receiver = receiver;
    }

    #region ReceiverEvent
    
    public Observable<Unit> OnConnected => receiver.OnConnected;
    public Observable<MajorityGameQuestion> OnAskedQuestion => receiver.OnAskedQuestion;
    public Observable<(Ulid AnswerPlayerUlid, int Index)> OnSelected => receiver.OnSelected;
    public Observable<MajorityGameResult> OnResult => receiver.OnResult;

    #endregion

    public async ValueTask AskQuestion(string questionText, params string[] choices)
    {
        await hub.AskQuestion(questionText, choices);
    }

    public async ValueTask Select(int index)
    {
        await hub.Select(index);
    }

    public async ValueTask ResultOpen()
    {
        await hub.ResultOpen();
    }
    
    public static async ValueTask<MajorityGameHub> CreateAsync(Player player, Ulid roomUlid, CancellationToken cancellationToken = default)
    {
        var receiver = new MajorityGameReceiver();
        var (hub, channel) = await StreamingHubFactory.CreateAsync<IMajorityGameHub, IMajorityGameReceiver>(receiver, player.Ulid, roomUlid, cancellationToken);
        return new(player, channel, hub, receiver);
    }

    private class MajorityGameReceiver : IMajorityGameReceiver, IDisposable
    {
        public readonly Subject<Unit> OnConnected = new();
        public readonly Subject<MajorityGameQuestion> OnAskedQuestion = new();
        public readonly Subject<(Ulid AnswerPlayerUlid, int Index)> OnSelected = new();
        public readonly Subject<MajorityGameResult> OnResult = new();

        void IMajorityGameReceiver.OnConnected()
        {
            OnConnected.OnNext(Unit.Default);
        }

        void IMajorityGameReceiver.OnAskedQuestion(MajorityGameQuestion question)
        {
            OnAskedQuestion.OnNext(question);
        }

        void IMajorityGameReceiver.OnSelected(Ulid answerPlayerUlid, int index)
        {
            OnSelected.OnNext((answerPlayerUlid, index));
        }

        void IMajorityGameReceiver.OnResult(MajorityGameResult result)
        {
            OnResult.OnNext(result);
        }

        public void Dispose()
        {
            OnConnected.Dispose();
            OnAskedQuestion.Dispose();
            OnSelected.Dispose();
            OnResult.Dispose();
        }
    }

    public async void Dispose()
    {
        channel.Dispose();
        receiver.Dispose();
        await channel.ShutdownAsync();
        await hub.DisposeAsync();
    }
}
