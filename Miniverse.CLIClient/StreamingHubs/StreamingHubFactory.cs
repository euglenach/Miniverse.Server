using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MiniverseShared.MessagePackObjects;
using MiniverseShared.StreamingHubs;

namespace Miniverse.CLIClient.StreamingHubs;

public class StreamingHubFactory
{
    public static ValueTask<(THub hub, GrpcChannel channel)> CreateAsync<THub, TReceiver>(TReceiver receiver, Ulid playerUlid, Ulid roomUlid, CancellationToken cancellationToken = default)
        where THub: IStreamingHub<THub, TReceiver>
    {
        var headers = new Metadata { { "player_ulid", playerUlid.ToString() }, {"room_ulid", roomUlid.ToString() } };

        return CreateAsync<THub, TReceiver>(receiver, headers, cancellationToken);
    }
    
    public static async ValueTask<(THub hub, GrpcChannel channel)> CreateAsync<THub, TReceiver>(TReceiver receiver, Metadata? header = null, CancellationToken cancellationToken = default)
        where THub: IStreamingHub<THub, TReceiver>
    {
        // todo: アドレスはサービスディスカバリからもらう
        
        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress("http://localhost:5209", new GrpcChannelOptions(){HttpHandler =  new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator }});
        
        // Create a proxy to call the server transparently.
        var hubClient = await StreamingHubClient.ConnectAsync<THub, TReceiver>(channel, receiver, option: new(header), cancellationToken : cancellationToken);

        return (hubClient, channel);
    }
}
