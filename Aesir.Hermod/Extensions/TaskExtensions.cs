namespace Aesir.Hermod.Extensions;

internal static class TaskExtensions
{
    internal static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, Action? onTimeout = null)
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();

        if (await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token)) == task)
        {
            timeoutCancellationTokenSource.Cancel();
            return await task;
        }
        else
        {
            onTimeout?.Invoke();
            throw new TimeoutException($"The provided task has timed out after {timeout:hh\\:mm\\:ss}.");
        }
    }
}
