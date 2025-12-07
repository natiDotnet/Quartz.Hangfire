using System.Linq.Expressions;
using Hangfire.Common;
using Hangfire.Storage;

namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Enqueues a job for execution using Quartz scheduler
    /// </summary>
    /// <param name="factory">The scheduler factory used to get the Quartz scheduler instance</param>
    /// <param name="job">The Hangfire job to be executed</param>
    /// <param name="queue">Optional queue name. If not specified, a new GUID will be used</param>
    /// <param name="delay">Optional delay before the job starts executing</param>
    /// <param name="enqueueAt">Optional specific date/time when the job should start executing</param>
    /// <returns>The TriggerKey of the scheduled job</returns>
    private static async Task<TriggerKey> InternalEnqueue(
        ISchedulerFactory factory,
        Job job,
        string? queue = null,
        TimeSpan? delay = null,
        DateTimeOffset? enqueueAt = null)
    {
        JobDataMap jobData = new()
        {
            {
                "Data", InvocationData.SerializeJob(job)
            }
        };

        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity(queue ?? Guid.NewGuid().ToString())
            .UsingJobData(jobData)
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue || enqueueAt.HasValue)
            triggerBuilder.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            triggerBuilder.StartNow();

        ITrigger trigger = triggerBuilder
            .WithIdentity($"{queue ?? Guid.NewGuid().ToString()}-trigger")
            .Build();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.ScheduleJob(expressionJob, trigger);
        return trigger.Key;
    }
}