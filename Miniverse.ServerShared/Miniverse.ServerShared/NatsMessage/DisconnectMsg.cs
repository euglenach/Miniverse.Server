using System;
using MessagePack;

namespace Miniverse.ServerShared.NatsMessage;

[MessagePackObject]
public readonly record struct DisconnectMsg([property: Key(0)] Ulid PlayerUlid);