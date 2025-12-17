using Hangfire;

namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A job execution step that handles job continuations.
/// It checks for a "NextTrigger" in the job data and schedules it based on the continuation options.
/// </summary>
public class ContinueJob : IJobExecutionStep
{
    /// <summary>
    /// Called when a job is about to be executed. This implementation does nothing.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    /// <returns>A completed task.</returns>
    public Task OnExecuting(IJobExecutionContext context, CancellationToken cancellationToken, Func<Task> next)
        => Task.CompletedTask;
    
    /// <summary>
    /// Called when a job has been executed.
    /// Handles the scheduling of the next job in a continuation chain.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="exception">The exception that occurred during job execution, if any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    public async Task OnExecuted(IJobExecutionContext context, JobExecutionException? exception, CancellationToken cancellationToken, Func<Task> next)
    {
        var jobData = context.Trigger.JobDataMap;

        // No continuation?
        if (!jobData.TryGetString("NextTrigger", out var nextKey) || string.IsNullOrWhiteSpace(nextKey))
        {
            return;
        }

        var options = JobContinuationOptions.OnlyOnSucceededState;
        if (jobData.TryGetInt("Options", out var value))
        {
            options = (JobContinuationOptions)value;
        }
        JobContinuationOptions state = exception is null ? JobContinuationOptions.OnlyOnSucceededState : JobContinuationOptions.OnlyOnDeletedState;

        var oldTrigger = await context.Scheduler.GetTrigger(new TriggerKey(nextKey), cancellationToken);
        if (options != JobContinuationOptions.OnAnyFinishedState && options != state)
        {
            if (oldTrigger is not null)
            {
                await CleanupTriggers(context.Scheduler, oldTrigger.Key.Name);
            }
            return;
        }
        var trigger = oldTrigger!.GetTriggerBuilder()
            .ForJob(nextKey)
            .StartNow()
            .Build();
        await context.Scheduler.RescheduleJob(oldTrigger.Key, trigger, cancellationToken);
    }
    
    /// <summary>
    /// Schedules a job to clean up a trigger.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="triggerKey">The key of the trigger to clean up.</param>
    private static async Task CleanupTriggers(IScheduler scheduler, string triggerKey)
    {
        var job = JobBuilder.Create<CleanupTriggerJob>()
            .WithIdentity(nameof(CleanupTriggerJob))
            .UsingJobData(new JobDataMap { { "TriggerKey", triggerKey } })
            .Build();
        var trigger = TriggerBuilder.Create().StartNow().Build();
        await scheduler.ScheduleJob(job, trigger);
    }
}
