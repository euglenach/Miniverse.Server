using Cysharp.Threading;
using MiniverseShared.Utility;

namespace Miniverse.LogicLooper.LooperTasks;

public class LooperHelper : IDisposable
{
    public long CurrentFrame { get; private set; }
    private readonly List<ILooperItem?> looperItems = [];
    private readonly CancellationTokenSource disposeCancellation = new();
    private readonly Lock listLock = new();
    
    public void Update(in LogicLooperActionContext context)
    {
        CurrentFrame = context.CurrentFrame;
        
        using var removeIndexList = new TempList<int>();

        lock(listLock)
        {
            for(var i = 0; i < looperItems.Count; i++)
            {
                var item = looperItems[i];
                if(item != null)
                {
                    try
                    {
                        if(!item.Update(context, disposeCancellation.Token))
                        {
                            looperItems[i] = null;
                            removeIndexList.Add(i);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch(Exception e)
                    {
                        looperItems[i] = null;
                        if(!removeIndexList.Span.Contains(i)) removeIndexList.Add(i);
                        try
                        {
                            LooperTaskScheduler.PublishUnobservedTaskException(e);
                        }
                        catch { }
                    }
                }
                else
                {
                    removeIndexList.Add(i);
                }
            }
        
            // 逆から回してインデックスがずれないようにする
            for(var i = removeIndexList.Count - 1; i >= 0; i--)
            {
                looperItems.RemoveAt(removeIndexList[i]);
            }
        }
    }

    public void AddItem(ILooperItem item)
    {
        lock(listLock)
        {
            var index = looperItems.IndexOf(null);
            if(index >= 0)
            {
                looperItems[index] = item;
            }
            else
            {
                looperItems.Add(item);
            }
        }
    }

    public void Dispose()
    {
        foreach(var item in looperItems)
        {
            if(item is IDisposable disposable) disposable.Dispose();
        }
        looperItems.Clear();
        disposeCancellation.Cancel();
        disposeCancellation.Dispose();
    }
}
