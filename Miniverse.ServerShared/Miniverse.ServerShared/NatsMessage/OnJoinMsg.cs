using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage;

[MessagePackObject]
public readonly record struct OnJoinMsg(Ulid RoomUlid, Player Player)
{
    [Key(0)] public readonly Ulid RoomUlid = RoomUlid;
    [Key(1)] public readonly Player Player = Player;
}

// [MessagePackObject]
// public class OnJoinMsg
// {
//     [Key(0)] public Ulid RoomUlid{get;set;}
//     [Key(1)] public Player Player{get;set;}
// }