using System;
using MessagePack;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct ResultOpenMsg([property : Key(0)] Ulid PlayerUlid);
