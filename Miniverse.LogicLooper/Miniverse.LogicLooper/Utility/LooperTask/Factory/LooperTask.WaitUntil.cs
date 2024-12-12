using Cysharp.Threading;

namespace Miniverse.LogicLooper.LooperTasks;

public partial class LooperTask
{
    public ValueTask WaitUntil(Func<bool> predicate, CancellationToken cancellationToken = default)
    {
        var task = WaitUntilLooperItem.CreateValueTask(predicate, cancellationToken, out var item);
        looperHelper.AddItem(item);
        return task;
    }
    
    public ValueTask WaitUntil<T1>(T1 state, Func<T1, bool> predicate, CancellationToken cancellationToken = default)
    {
        var task = WaitUntilLooperItemWithState1<T1>.CreateValueTask(state, predicate, cancellationToken, out var item);
        looperHelper.AddItem(item);
        return task;
    }
}

file class WaitUntilLooperItem(Func<bool> predicate, ManualResetValueTaskSource core, CancellationToken cancellationToken) : FactoryLooperItem(core)
{
    public static ValueTask CreateValueTask(Func<bool> predicate, CancellationToken cancellationToken, out WaitUntilLooperItem item)
    {
        var source = ManualResetValueTaskSource.Create();
        item = new WaitUntilLooperItem(predicate, source, cancellationToken);
        return new ValueTask(source, source.Version);
    }
    
    public override bool Update(in LogicLooperActionContext context, CancellationToken token)
    {
        if(cancellationToken.IsCancellationRequested)
        {
            core.TrySetCanceled(cancellationToken);
            return false;
        }

        if(token.IsCancellationRequested)
        {
            core.TrySetCanceled(token);
            return false;
        }

        if(context.CancellationToken.IsCancellationRequested)
        {
            core.TrySetCanceled(context.CancellationToken);
            return false;
        }

        if(predicate())
        {
            core.TrySetResult();
            return false;
        }
        
        return true;
    }
}

file class WaitUntilLooperItemWithState1<T>(T state, Func<T, bool> predicate, ManualResetValueTaskSource core, CancellationToken cancellationToken) : FactoryLooperItem(core)
{
    public static ValueTask CreateValueTask(T state, Func<T, bool> predicate, CancellationToken cancellationToken, out WaitUntilLooperItemWithState1<T> item)
    {
        var source = ManualResetValueTaskSource.Create();
        item = new WaitUntilLooperItemWithState1<T>(state, predicate, source, cancellationToken);
        return new ValueTask(source, source.Version);
    }
    
    public override bool Update(in LogicLooperActionContext context, CancellationToken token)
    {
        if(cancellationToken.IsCancellationRequested)
        {
            core.TrySetCanceled(cancellationToken);
            return false;
        }

        if(token.IsCancellationRequested)
        {
            core.TrySetCanceled(token);
            return false;
        }

        if(context.CancellationToken.IsCancellationRequested)
        {
            core.TrySetCanceled(context.CancellationToken);
            return false;
        }
        
        if(predicate(state))
        {
            core.TrySetResult();
            return false;
        }
        
        return true;
    }
}