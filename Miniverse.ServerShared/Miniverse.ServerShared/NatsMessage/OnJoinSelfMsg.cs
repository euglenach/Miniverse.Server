using System;
using MessagePack;
using MiniverseShared.MessagePackObjects;

namespace Miniverse.ServerShared.NatsMessage;

[MessagePackObject]
public readonly record struct OnJoinSelfMsg(MajorityGameRoomInfo RoomInfo, Player Player)
{
    [Key(0)] public readonly MajorityGameRoomInfo RoomInfo = RoomInfo;
    [Key(1)] public readonly Player Player = Player;
}

// [MessagePackObject]
// public class OnJoinSelfMsg
// {
//     [Key(0)] public MajorityGameRoomInfo RoomInfo{get;set;}
//     [Key(1)] public Player Player{get;set;}
// }