using System;
using System.Threading.Tasks;
using Xunit;
namespace GIT.Utilities.Test;
public class RunWithTimedCallbackTests
{
    [Fact]
    public async Task TestRunWithTimedCallbackUsingAction()
    {
        int tickCount = 0;
        Action tickDelegate = () => tickCount++;

        var mainTask = Task.Delay(TimeSpan.FromSeconds(5));
        await mainTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickDelegate);

        // Since we're ticking every 1 second for a 5-second main task, we expect approximately 5 ticks.
        Assert.True(tickCount >= 4 && tickCount <= 6, $"Unexpected tickCount: {tickCount}");
    }

    [Fact]
    public async Task TestRunWithTimedCallbackUsingFuncTask()
    {
        int tickCount = 0;
        Func<Task> tickDelegate = async () =>
        {
            tickCount++;
            await Task.CompletedTask; // Just to make it asynchronous.
        };

        var mainTask = Task.Delay(TimeSpan.FromSeconds(5));
        await mainTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickDelegate);

        // Since we're ticking every 1 second for a 5-second main task, we expect approximately 5 ticks.
        Assert.True(tickCount >= 4 && tickCount <= 6, $"Unexpected tickCount: {tickCount}");
    }
    [Fact]
    public async Task RunWithTicker_DoesNotDeadlockWithQuickTask()
    {
        var tickCount = 0;
        Action tickAction = () => tickCount++;

        var quickTask = Task.CompletedTask;

        // Run the quick task with a tick interval of 1 second
        var runWithTickerTask = quickTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickAction);

        // Wait with a timeout to detect potential deadlock
        var completedTask = await Task.WhenAny(runWithTickerTask, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Equal(runWithTickerTask, completedTask); // Ensure that the runWithTickerTask is the one that completed
    }

    [Fact]
    public async Task TestRunWithTimedCallbackUsingTaskT()
    {
        int tickCount = 0;
        Action tickDelegate = () => tickCount++;

        var mainTask = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return "Finished";
        });

        var result = await mainTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickDelegate);

        Assert.Equal("Finished", result);

        // Since we're ticking every 1 second for a 5-second main task, we expect approximately 5 ticks.
        Assert.True(tickCount >= 4 && tickCount <= 6, $"Unexpected tickCount: {tickCount}");
    }

}
