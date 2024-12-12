using System;
using MessagePack;

namespace Miniverse.ServerShared.NatsMessage;

[MessagePackObject]
public readonly record struct OnLeaveRoomMsg([property: Key(0)] Ulid PlayerUlid);