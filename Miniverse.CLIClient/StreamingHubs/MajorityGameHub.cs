﻿using Grpc.Net.Client;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;
using R3;

namespace Miniverse.CLIClient.StreamingHubs;

public class MajorityGameHub
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
    
    public Observable<MajorityGameQuestion> OnAskedQuestion => receiver.OnAskedQuestion;
    public Observable<(Ulid AnswerPlayerUlid, int Index)> OnSelected => receiver.OnSelected;
    public Observable<MajorityGameResult> OnResult => receiver.OnResult;

    #endregion
    
    
    
    public static async ValueTask<MajorityGameHub> CreateAsync(Player player, Ulid roomUlid)
    {
        var receiver = new MajorityGameReceiver();
        var (hub, channel) = await StreamingHubFactory.CreateAsync<IMajorityGameHub, IMajorityGameReceiver>(receiver, player.Ulid, roomUlid);
        return new(player, channel, hub, receiver);
    }

    private class MajorityGameReceiver : IMajorityGameReceiver
    {
        public readonly Subject<MajorityGameQuestion> OnAskedQuestion = new();
        public readonly Subject<(Ulid AnswerPlayerUlid, int Index)> OnSelected = new();
        public readonly Subject<MajorityGameResult> OnResult = new();
        
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
    }
}