using Cysharp.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

namespace Miniverse.ServerShared;

public static class MessagePackOptionRegister
{
    public static void Register()
    {
        StaticCompositeResolver.Instance.Register(
            StandardResolver.Instance,
            UnityResolver.Instance, 
            UlidMessagePackResolver.Instance);
        
        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
    }
}
