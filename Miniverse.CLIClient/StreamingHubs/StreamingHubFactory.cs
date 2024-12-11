using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;

namespace Miniverse.CLIClient.StreamingHubs;

public class StreamingHubFactory
{
    public static async ValueTask<(THub hub, GrpcChannel channel)> CreateAsync<THub, TReceiver>(TReceiver receiver)
        where THub: IStreamingHub<THub, TReceiver>
    {
        // todo: アドレスはサービスディスカバリからもらう
        
        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress("http://localhost:5209", new GrpcChannelOptions(){HttpHandler =  new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }});

        // Create a proxy to call the server transparently.
        var hubClient = await StreamingHubClient.ConnectAsync<THub, TReceiver>(channel, receiver);

        return (hubClient, channel);
    }
}
