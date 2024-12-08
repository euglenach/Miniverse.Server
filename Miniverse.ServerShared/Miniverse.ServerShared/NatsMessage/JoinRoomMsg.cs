using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage;

[MessagePackObject]
public readonly record struct JoinRoomMsg(Ulid RoomUlid, Player Player)
{
    [Key(0)] public readonly Ulid RoomUlid = RoomUlid;
    [Key(1)] public readonly Player Player = Player;
}