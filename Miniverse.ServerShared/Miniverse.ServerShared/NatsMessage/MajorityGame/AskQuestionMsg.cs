using System;
using MessagePack;

namespace Miniverse.ServerShared.NatsMessage.MajorityGame;

[MessagePackObject]
public readonly record struct AskQuestionMsg([property : Key(0)] Ulid PlayerUlid, [property : Key(1)] string QuestionText, [property : Key(2)] string[] Choices);
