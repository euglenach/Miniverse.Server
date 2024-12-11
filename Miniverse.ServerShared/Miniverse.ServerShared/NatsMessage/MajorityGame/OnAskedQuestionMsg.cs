using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct OnAskedQuestionMsg([property : Key(0)] MajorityGameQuestion Question);