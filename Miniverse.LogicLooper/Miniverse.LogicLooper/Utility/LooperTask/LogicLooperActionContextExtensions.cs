using Cysharp.Threading;

namespace Miniverse.LogicLooper.LooperTasks;

public static class LogicLooperActionContextExtensions
{
    public static LooperTask CreateLooperTask(this ILogicLooper looper, LooperHelper? looperHelper = null, LooperActionOptions? options = null)
    {
        looperHelper ??= new LooperHelper();
        var looperTask = new LooperTask(looperHelper);
        
        looper.RegisterActionAsync(static (in LogicLooperActionContext ctx, LooperHelper helper) =>
        {
            if(ctx.CancellationToken.IsCancellationRequested) return false;
            
            helper.Update(ctx);
            return true;
        }, looperHelper, options ?? LooperActionOptions.Default);

        return looperTask;
    }
    
    public static LooperTask CreateLooperTask(this LogicLooperActionContext context, LooperHelper? looperHelper = null, LooperActionOptions? options = null)
    {
        looperHelper ??= new LooperHelper();
        var looperTask = new LooperTask(looperHelper);
        options ??= new((int)context.Looper.TargetFrameRate);
        
        context.Looper.RegisterActionAsync(static (in LogicLooperActionContext ctx, (LooperHelper, LogicLooperActionContext) state) =>
        {
            var (helper, context) = state;
            if(ctx.CancellationToken.IsCancellationRequested) return false;
            if(context.CancellationToken.IsCancellationRequested) return false;
            
            helper.Update(ctx);
            return true;
        }, (looperHelper, context), options);

        return looperTask;
    }
}
