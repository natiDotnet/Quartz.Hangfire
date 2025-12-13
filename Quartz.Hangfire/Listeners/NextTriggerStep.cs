using Hangfire;

namespace Quartz.Hangfire.Listeners;

public class NextTriggerStep : IJobExecutionStep
{
    public Task OnExecuting(IJobExecutionContext context, CancellationToken cancellationToken, Func<Task> next)
        => Task.CompletedTask;
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
