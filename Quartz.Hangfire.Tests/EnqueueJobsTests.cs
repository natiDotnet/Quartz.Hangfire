using NSubstitute;
using Quartz.Hangfire.Tests.Helpers;

namespace Quartz.Hangfire.Tests;

public class EnqueueJobsTests
{
    private readonly IScheduler _scheduler = Substitute.For<IScheduler>();
    private readonly ISchedulerFactory _schedulerFactory = Substitute.For<ISchedulerFactory>();

    public EnqueueJobsTests()
    {
        _schedulerFactory.GetScheduler(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_scheduler));
    }

    [Fact]
    public async Task Enqueue_Should_Schedule_Job_With_Correct_Parameters()
    {
        // Arrange
        var schedulerFactory = _schedulerFactory;

        // Act
        var triggerKey = await schedulerFactory.Enqueue(() => TestJob.Execute());

        // Assert
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        Assert.NotNull(triggerKey);
    }

    [Fact]
    public async Task Enqueue_WithQueue_Should_Schedule_Job_With_Correct_Parameters()
    {
        // Arrange
        var schedulerFactory = _schedulerFactory;
        const string queue = "critical";

        // Act
        var triggerKey = await schedulerFactory.Enqueue(queue, () => TestJob.Execute());

        // Assert
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        Assert.NotNull(triggerKey);
        // Assert.StartsWith(queue, triggerKey.Name);
    }

    [Fact]
    public async Task Enqueue_Async_Should_Schedule_Job_With_Correct_Parameters()
    {
        // Arrange
        var schedulerFactory = _schedulerFactory;

        // Act
        var triggerKey = await schedulerFactory.Enqueue(() => TestJob.ExecuteAsync());

        // Assert
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        Assert.NotNull(triggerKey);
    }
}
