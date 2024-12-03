using System.Collections.Concurrent;
using Cysharp.Threading;

namespace Miniverse.LogicLooper;

public class LifeGameLoop
{
    private readonly ILogger logger;
    public static ConcurrentBag<LifeGameLoop> All { get; } = new();
    
    /// <summary>
    /// Create a new life-game loop and register into the LooperPool.
    /// </summary>
    /// <param name="looperPool"></param>
    /// <param name="logger"></param>
    public static void CreateNew(ILogicLooperPool looperPool, ILogger logger)
    {
        var gameLoop = new LifeGameLoop(logger);
        looperPool.RegisterActionAsync(gameLoop.UpdateFrame);
    }
    
    private LifeGameLoop(ILogger logger)
    {
        logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        // Id = Interlocked.Increment(ref _gameLoopSeq);
        // World = new World(64, 64);
        // World.SetPattern(Patterns.GliderGun, 10, 10);
        //
        // logger.LogInformation($"{nameof(LifeGameLoop)}[{Id}]: Register");

        All.Add(this);
    }
    
    public bool UpdateFrame(in LogicLooperActionContext ctx)
    {
        logger.LogInformation("Looper Update");
        
        if (ctx.CancellationToken.IsCancellationRequested)
        {
            // If LooperPool begins shutting down, IsCancellationRequested will be `true`.
            // logger.LogInformation($"{nameof(LifeGameLoop)}[{Id}]: Shutdown");
            return false;
        }

        // Update the world every update cycle.
        // World.Update();

        // return World.AliveCount != 0;
        return true;
    }
}
