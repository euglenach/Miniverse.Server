using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct OnResultMsg([property : Key(0)] MajorityGameResult Result);