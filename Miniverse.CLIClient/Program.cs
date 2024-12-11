// See https://aka.ms/new-console-template for more information

using FluentAssertions;
using Miniverse.CLIClient;
using ZLogger;

MessagePackOptionRegister.Register();

LogManager.Global.ZLogDebug($"aaaaaaaaaaa");

var test = new MajorityGame();

await test.Run();