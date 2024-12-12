using Cysharp.Threading;

namespace Miniverse.LogicLooper.LooperTasks;

public interface ILooperItem : IDisposable
{
    bool Update(in LogicLooperActionContext context, CancellationToken cancellationToken);
}