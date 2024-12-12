using System;
using System.Threading;
using System.Threading.Tasks.Sources;
using MessagePack;
using Microsoft.Extensions.ObjectPool;

namespace Miniverse.LogicLooper.LooperTasks;

public class ManualResetValueTaskSource : IValueTaskSource
{
    private ManualResetValueTaskSourceCore<Nil> core;
    public bool RunContinuationsAsynchronously { get => core.RunContinuationsAsynchronously; set => core.RunContinuationsAsynchronously = value; }
    public short Version => core.Version;
    private ManualResetValueTaskSource() {}
    
    public static ManualResetValueTaskSource Create()
    {
        var source = pool.Get();
        return source;
    }
    
    void IValueTaskSource.GetResult(short token)
    {
        try
        {
            core.GetResult(token);
        }
        finally
        {
            pool.Return(this);
        }
    }

    void Reset() => core.Reset();

    public bool TrySetResult()
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetResult(Nil.Default);
        return true;
    }

    public bool TrySetException(Exception error)
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(error);
        return true;
    }

    public bool TrySetCanceled()
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(new OperationCanceledException());
        return true;
    }

    public bool TrySetCanceled(CancellationToken cancellationToken)
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(new OperationCanceledException(cancellationToken));
        return true;
    }

    public ValueTaskSourceStatus GetStatus(short token) => core.GetStatus(token);
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => core.OnCompleted(continuation, state, token, flags);
    
    private static readonly ObjectPool<ManualResetValueTaskSource> pool = new DefaultObjectPool<ManualResetValueTaskSource>(new PooledObjectPolicy());
    
    private class PooledObjectPolicy : PooledObjectPolicy<ManualResetValueTaskSource>
    {
        public override ManualResetValueTaskSource Create() => new();
        public override bool Return(ManualResetValueTaskSource source)
        {
            source.Reset();
            return true;
        }
    }
}

public class ManualResetValueTaskSource<T> : IValueTaskSource<T>
{
    private ManualResetValueTaskSourceCore<T> core;
    public bool RunContinuationsAsynchronously { get => core.RunContinuationsAsynchronously; set => core.RunContinuationsAsynchronously = value; }
    public short Version => core.Version;
    private ManualResetValueTaskSource() {}
    
    public static ManualResetValueTaskSource<T> Create()
    {
        var source = pool.Get();
        return source;
    }
    
    T IValueTaskSource<T>.GetResult(short token)
    {
        try
        {
            return core.GetResult(token);
        }
        finally
        {
            pool.Return(this);
        }
    }

    void Reset() => core.Reset();

    public bool TrySetResult(T result)
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetResult(result);
        return true;
    }

    public bool TrySetException(Exception error)
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(error);
        return true;
    }

    public bool TrySetCanceled()
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(new OperationCanceledException());
        return true;
    }

    public bool TrySetCanceled(CancellationToken cancellationToken)
    {
        if(GetStatus(Version) != ValueTaskSourceStatus.Pending) return false;
        core.SetException(new OperationCanceledException(cancellationToken));
        return true;
    }

    public ValueTaskSourceStatus GetStatus(short token) => core.GetStatus(token);
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => core.OnCompleted(continuation, state, token, flags);
    
    private static readonly ObjectPool<ManualResetValueTaskSource<T>> pool = new DefaultObjectPool<ManualResetValueTaskSource<T>>(new PooledObjectPolicy());
    
    private class PooledObjectPolicy : PooledObjectPolicy<ManualResetValueTaskSource<T>>
    {
        public override ManualResetValueTaskSource<T> Create() => new();

        public override bool Return(ManualResetValueTaskSource<T> source)
        {
            source.Reset();
            return true;
        }
    }
}