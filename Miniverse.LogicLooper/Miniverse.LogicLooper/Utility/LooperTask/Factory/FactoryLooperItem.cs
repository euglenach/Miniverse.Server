using Cysharp.Threading;

namespace Miniverse.LogicLooper.LooperTasks;

public abstract class FactoryLooperItem(ManualResetValueTaskSource core) : ILooperItem
{
    protected readonly short version = core.Version;

    void IDisposable.Dispose()
    {
        if(version != core.Version) return;
        core.TrySetCanceled();
        GC.SuppressFinalize(this);
    }

    public abstract bool Update(in LogicLooperActionContext context, CancellationToken cancellationToken);
}
