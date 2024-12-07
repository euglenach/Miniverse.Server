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
    
    public void Initialize(string url)
    {
        var options = NatsOpts.Default with
        {
            Url = string.IsNullOrEmpty(url)? NatsOpts.Default.Url : url,
            SerializerRegistry = NatsMessagePackSerializerRegistry.Default,
        };
        
        connectionPool = new NatsConnectionPool(options);
    }

    public ValueTask Publish<T>(string key, T value)
    {
        if(connectionPool is null) throw new InvalidOperationException("No connection pool available.");
        return connectionPool.GetConnection().PublishAsync(key, value);
    }
    
    public ValueTask Publish<T>(T value)
    {
        return Publish(typeof(T).FullName!, value);
    }

    public async IAsyncEnumerable<T> Subscribe<T>(string key)
    {
        if(connectionPool is null) throw new InvalidOperationException("No connection pool available.");
        await foreach(var msg in connectionPool.GetConnection().SubscribeAsync<T>(key))
        {
            yield return msg.Data!;
        }
    }
    
    public IAsyncEnumerable<T> Subscribe<T>()
    {
        return Subscribe<T>(typeof(T).FullName!);
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