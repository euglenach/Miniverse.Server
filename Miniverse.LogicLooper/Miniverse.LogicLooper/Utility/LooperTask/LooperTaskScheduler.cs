namespace Miniverse.LogicLooper.LooperTasks;

public static class LooperTaskScheduler
{
    public static bool PropagateOperationCanceledException = false;
    private static Action<Exception> unhandledExceptionHandler = e => Console.WriteLine("UnhandledException: " + e);
    
    public static void PublishUnobservedTaskException(Exception? ex)
    {
        if(ex is null) return;
        
        // OperationCanceledExceptionは無視
        if(!PropagateOperationCanceledException && ex is OperationCanceledException) return;

        unhandledExceptionHandler(ex);
    }
    
    public static void RegisterUnhandledExceptionHandler(Action<Exception> unhandledExceptionHandler)
    {
        LooperTaskScheduler.unhandledExceptionHandler = unhandledExceptionHandler;
    }
}