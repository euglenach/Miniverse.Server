using System;
using System.Threading;
using System.Threading.Tasks;

namespace Miniverse.ServerShared.Utility;

public class AsyncLock
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public async ValueTask<LockReleaser> EnterScope()
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        return new LockReleaser(this.semaphore);
    }

    public readonly struct LockReleaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
