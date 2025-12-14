using NSubstitute;
using Quartz.Hangfire.Queue;
using Quartz.Hangfire.Tests.Helpers;

namespace Quartz.Hangfire.Tests;

public class ContinueJobWithTests
{
    private readonly IScheduler _scheduler = Substitute.For<IScheduler>();
    private readonly ISchedulerFactory _schedulerFactory = Substitute.For<ISchedulerFactory>();
    private readonly ITrigger _trigger = Substitute.For<ITrigger>();
    private readonly IJobDetail _jobDetail = Substitute.For<IJobDetail>();

    public ContinueJobWithTests()
    {
        _schedulerFactory.GetScheduler(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_scheduler));
        
        _jobDetail.JobDataMap.Returns(new JobDataMap());
        _trigger.JobDataMap.Returns(new JobDataMap());
        _trigger.GetTriggerBuilder().Returns(TriggerBuilder.Create());
    }

    [Fact]
    public async Task ContinueJobWith_Should_Schedule_Continuation()
    {
        // Arrange
        var parentTriggerKey = new TriggerKey("parentJob");
        var parentJobKey = new JobKey("parentJobKey");

        _scheduler.GetTrigger(parentTriggerKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult(_trigger));
        _trigger.JobKey.Returns(parentJobKey);
        _scheduler.GetJobDetail(parentJobKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult(_jobDetail));
        
        _jobDetail.Durable.Returns(true); 

        // Act
        var result = await _schedulerFactory.ContinueJobWith(parentTriggerKey, () => TestJob.Execute());

        // Assert
        Assert.True(result);
        await _scheduler.Received(1).ScheduleJob(Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        await _scheduler.Received(1).RescheduleJob(parentTriggerKey, _trigger, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ContinueJobWith_Should_Return_False_If_Parent_Not_Found()
    {
        // Arrange
        var parentTriggerKey = new TriggerKey("parentJob");
        _scheduler.GetTrigger(parentTriggerKey, Arg.Any<CancellationToken>())!
            .Returns(Task.FromResult<ITrigger>(null!));
        
        // Act
        var result = await _schedulerFactory.ContinueJobWith(parentTriggerKey, () => TestJob.Execute());

        // Assert
        Assert.False(result);
        await _scheduler.DidNotReceive().ScheduleJob(Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        await _scheduler.DidNotReceive().RescheduleJob(Arg.Any<TriggerKey>(), Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContinueJobWith_WithQueue_Should_Schedule_Continuation()
    {
        // Arrange
        var parentTriggerKey = new TriggerKey("parentJob");
        var parentJobKey = new JobKey("parentJobKey");
        const string queue = "critical";
        QuartzQueues.Configure("critical", "high", "default", "low");

        _scheduler.GetTrigger(parentTriggerKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult(_trigger));
        _trigger.JobKey.Returns(parentJobKey);
        _scheduler.GetJobDetail(parentJobKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult(_jobDetail));
        _jobDetail.Durable.Returns(true);

        // Act
        var result = await _schedulerFactory.ContinueJobWith(parentTriggerKey, queue, () => TestJob.Execute());

        // Assert
        Assert.True(result);
        await _scheduler.Received(1).ScheduleJob(Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        await _scheduler.Received(1).RescheduleJob(parentTriggerKey, _trigger, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ContinueJobWith_Should_Return_False_If_Parent_JobDetail_Not_Found()
    {
        // Arrange
        var parentTriggerKey = new TriggerKey("parentJob");
        var parentJobKey = new JobKey("parentJobKey");

        _scheduler.GetTrigger(parentTriggerKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult(_trigger));
        _trigger.JobKey.Returns(parentJobKey);
        _scheduler.GetJobDetail(parentJobKey, Arg.Any<CancellationToken>())!.Returns(Task.FromResult<IJobDetail>(null!));

        // Act
        var result = await _schedulerFactory.ContinueJobWith(parentTriggerKey, () => TestJob.Execute());

        // Assert
        Assert.True(result);
        await _scheduler.Received(1).ScheduleJob(Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
        await _scheduler.Received(1).RescheduleJob(parentTriggerKey, _trigger, Arg.Any<CancellationToken>());
    }
}
