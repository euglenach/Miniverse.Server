using Cysharp.Threading;

namespace Miniverse.LogicLooper.LooperTasks;

public static class LogicLooperActionContextExtensions
{
    public static LooperTask CreateLooperTask(this LogicLooperActionContext context, LooperHelper? looperHelper = null)
    {
        looperHelper ??= new LooperHelper();
        var looperTask = new LooperTask(looperHelper);
        context.Looper.RegisterActionAsync(static (in LogicLooperActionContext ctx, (LooperHelper, LogicLooperActionContext) state) =>
        {
            var (looperHelper, originContext) = state;
            if(originContext.CancellationToken.IsCancellationRequested) return false;
            if(ctx.CancellationToken.IsCancellationRequested) return false;
            
            looperHelper.Update(ctx);
            return true;
        }, (looperHelper, context));

        return looperTask;
    }
}
