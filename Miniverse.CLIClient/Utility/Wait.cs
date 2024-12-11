namespace Miniverse.CLIClient.Utility;

public static class Wait
{
    
    
    public static async ValueTask<bool> WaitUntil(Func<bool> predicate, float timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (timeoutSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "Timeout must be non-negative.");
        
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(timeout).Token);
        
        try
        {
            while (!predicate())
            {
                if(cancellation.IsCancellationRequested) return false;
                
                await Task.Delay(50, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            if(cancellation.IsCancellationRequested)return false;
            throw;
        }

        return true;
    }
}
