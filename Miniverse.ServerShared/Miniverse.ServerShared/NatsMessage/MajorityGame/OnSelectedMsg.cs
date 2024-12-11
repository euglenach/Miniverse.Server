using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct OnSelectedMsg([property : Key(0)] Ulid TargetUlid, [property : Key(1)] Ulid SelectedPlayerUlid, [property : Key(2)] int Index);