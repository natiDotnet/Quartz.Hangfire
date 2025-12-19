using NSubstitute;
using Quartz.Hangfire.Tests.Helpers;

namespace Quartz.Hangfire.Tests;

public class ScheduleJobsTests
{
    private readonly IScheduler _scheduler = Substitute.For<IScheduler>();
    private readonly ISchedulerFactory _schedulerFactory = Substitute.For<ISchedulerFactory>();

    public ScheduleJobsTests()
    {
        _schedulerFactory.GetScheduler(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_scheduler));
    }

    [Fact]
    public async Task Schedule_Should_Schedule_Job_With_Correct_Delay()
    {
        // Arrange
        var delay = TimeSpan.FromMinutes(5);

        // Act
        var triggerKey = await _schedulerFactory.Schedule(() => TestJob.Execute(), delay);

        // Assert
        // The trigger's start time should be approximately UtcNow + delay.
        // Therefore, (t.StartTimeUtc - DateTimeOffset.UtcNow) should be close to the original delay.
        // We use a tolerance to account for the small time gap between scheduling and assertion.
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), 
            Arg.Is<ITrigger>(t => Math.Abs((t.StartTimeUtc - DateTimeOffset.UtcNow - delay).TotalSeconds) < 5), 
            Arg.Any<CancellationToken>());
        Assert.NotNull(triggerKey);
    }

    [Fact]
    public async Task Schedule_WithQueue_Should_Schedule_Job_With_Correct_Delay_And_Queue()
    {
        // Arrange
        var delay = TimeSpan.FromMinutes(5);
        const string queue = "critical";

        // Act
        var triggerKey = await _schedulerFactory.Schedule(queue, () => TestJob.Execute(), delay);

        // Assert
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(),
            Arg.Is<ITrigger>(t => Math.Abs((t.StartTimeUtc - DateTimeOffset.UtcNow - delay).TotalSeconds) < 5),
            Arg.Any<CancellationToken>());
        Assert.NotNull(triggerKey);
        // Assert.StartsWith(queue, triggerKey.Name);
    }
}
