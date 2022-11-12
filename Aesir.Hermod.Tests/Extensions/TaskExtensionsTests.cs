using Aesir.Hermod.Extensions;

namespace Aesir.Hermod.Tests.Extensions;

public class TaskExtensionsTests
{
    [Fact]
    public async void TimeoutAfter_ShouldReturnWhenTaskCompletes()
    {
        var taskSrc = new TaskCompletionSource<bool>();
        var task = taskSrc.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
        taskSrc.SetResult(true);
        var res = await task;
        Assert.True(res);
    }

    [Fact]
    public async void TimeoutAfter_ShouldTimeout()
    {
        var taskSrc = new TaskCompletionSource<bool>();
        await Assert.ThrowsAsync<TimeoutException>(async () => await taskSrc.Task.TimeoutAfter(TimeSpan.FromMilliseconds(1)));
    }

    [Fact]
    public async void TimeoutAfter_ShouldCallCallbackOnTimeout()
    {
        var taskSrc = new TaskCompletionSource<bool>();
        var res = false;
        await Assert.ThrowsAsync<TimeoutException>(async () => await taskSrc.Task.TimeoutAfter(TimeSpan.FromMilliseconds(1), () =>
        {
            res = true;
        }));
        Assert.True(res);
    }
}
