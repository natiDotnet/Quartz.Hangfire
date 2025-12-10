using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Quartz.Hangfire.Tests;

public class DeleteAndRescheduleTests
{
    private readonly IScheduler _scheduler = Substitute.For<IScheduler>();
    private readonly ISchedulerFactory _schedulerFactory = Substitute.For<ISchedulerFactory>();

    public DeleteAndRescheduleTests()
    {
        _schedulerFactory.GetScheduler(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_scheduler));
    }

    [Fact]
    public async Task Delete_Should_Delete_Job_And_Return_True_When_Job_Exists()
    {
        // Arrange
        var jobKey = new JobKey("jobToDelete");
        _scheduler.DeleteJob(jobKey, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        // Act
        var result = await _schedulerFactory.Delete(jobKey);

        // Assert
        Assert.True(result);
        await _scheduler.Received(1).DeleteJob(jobKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_Should_Return_False_When_Job_Does_Not_Exist()
    {
        // Arrange
        var jobKey = new JobKey("nonExistentJob");
        _scheduler.DeleteJob(jobKey, Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        // Act
        var result = await _schedulerFactory.Delete(jobKey);

        // Assert
        Assert.False(result);
        await _scheduler.Received(1).DeleteJob(jobKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reschedule_Should_Reschedule_Job_And_Return_True_When_Job_Exists()
    {
        // Arrange
        var triggerKey = new TriggerKey("jobToReschedule");
        var newFireTime = DateTimeOffset.UtcNow.AddMinutes(10);
        _scheduler.RescheduleJob(triggerKey, Arg.Any<ITrigger>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DateTimeOffset?>(newFireTime));

        // Act
        var result = await _schedulerFactory.Reschedule(triggerKey, TimeSpan.FromMinutes(10));

        // Assert
        Assert.NotNull(result);
        await _scheduler.Received(1).RescheduleJob(triggerKey, Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reschedule_Should_Return_False_When_Job_Does_Not_Exist()
    {
        // Arrange
        var triggerKey = new TriggerKey("nonExistentJob");
        _scheduler.RescheduleJob(triggerKey, Arg.Any<ITrigger>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<DateTimeOffset?>(null));

        // Act
        var result = await _schedulerFactory.Reschedule(triggerKey, TimeSpan.FromMinutes(10));

        // Assert
        Assert.Null(result);
        await _scheduler.Received(1).RescheduleJob(triggerKey, Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
    }
}
