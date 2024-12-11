using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct OnSelectedMsg([property : Key(0)] Ulid SelectedPlayerUlid, [property : Key(0)] int Index);