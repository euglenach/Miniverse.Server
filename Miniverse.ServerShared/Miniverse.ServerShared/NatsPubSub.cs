using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;
using NATS.Client.Hosting;

namespace Miniverse.ServerShared.Nats;

public class NatsPubSub : IAsyncDisposable
{
    private static NatsConnectionPool? connectionPool;
    
    public async ValueTask ConnectAsync(IServiceCollection serviceCollection)
    {
        serviceCollection.AddNats();
        var options = NatsOpts.Default with
        {
            SerializerRegistry = NatsMessagePackSerializerRegistry.Default
        };
        
        connectionPool = new NatsConnectionPool(options);
    }

    public async ValueTask Publish<T>(string roomUUID, T value)
    {
        if(connectionPool is null) return;
        await connectionPool.GetConnection().PublishAsync(roomUUID, value);
    }

    public IAsyncEnumerable<NatsMsg<T>> Subscribe<T>(string roomUUID)
    {
        if(connectionPool is null) throw new InvalidOperationException("No connection pool available.");
        return connectionPool.GetConnection().SubscribeAsync<T>(roomUUID);
    }

    public async ValueTask DisposeAsync()
    {
        if(connectionPool != null) await connectionPool.DisposeAsync();
    }
}

public class NatsMessagePackSerializerRegistry : INatsSerializerRegistry
{
    public static readonly NatsMessagePackSerializerRegistry Default = new();
    public INatsSerialize<T> GetSerializer<T>() => NatsMessagePackSerializer<T>.Default;

    public INatsDeserialize<T> GetDeserializer<T>() => NatsMessagePackSerializer<T>.Default;
}