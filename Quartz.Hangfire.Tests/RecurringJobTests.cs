using NSubstitute;
using Quartz.Hangfire.Tests.Helpers;

namespace Quartz.Hangfire.Tests;

public class RecurringJobTests
{
    private readonly IScheduler _scheduler = Substitute.For<IScheduler>();
    private readonly ISchedulerFactory _schedulerFactory = Substitute.For<ISchedulerFactory>();

    public RecurringJobTests()
    {
        _schedulerFactory.GetScheduler(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_scheduler));
    }

    [Fact]
    public async Task AddOrUpdate_Should_Schedule_Recurring_Job_With_Correct_Cron_Expression()
    {
        // Arrange
        const string cronExpression = "0 0/15 * * * ?"; // Every 15 minutes
        const string recurringJobId = "test-recurring-job";

        // Act
        await _schedulerFactory.AddOrUpdate(recurringJobId, () => TestJob.Execute(), cronExpression);

        // Assert
        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(),
            Arg.Is<ICronTrigger>(t => t.CronExpressionString == cronExpression),
            Arg.Any<CancellationToken>());
    }
}
