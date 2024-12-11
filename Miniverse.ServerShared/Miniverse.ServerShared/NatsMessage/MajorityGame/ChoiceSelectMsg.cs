using System;
using MessagePack;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct ChoiceSelectMsg([property : Key(0)] Ulid PlayerUlid, [property : Key(1)] int Index);
