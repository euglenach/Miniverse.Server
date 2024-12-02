using System.Buffers;
using MessagePack;
using NATS.Client.Core;

namespace Miniverse.ServerShared.Nats;

public class NatsMessagePackSerializer<T> : INatsSerializer<T>
{
    public static readonly NatsMessagePackSerializer<T> Default = new();
    
    public void Serialize(IBufferWriter<byte> bufferWriter, T value)
    {
        MessagePackSerializer.Serialize(bufferWriter, value);
    }

    public T? Deserialize(in ReadOnlySequence<byte> buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer);
    }

    public INatsSerializer<T> CombineWith(INatsSerializer<T> next)
    {
        throw new System.NotImplementedException();
    }
}
