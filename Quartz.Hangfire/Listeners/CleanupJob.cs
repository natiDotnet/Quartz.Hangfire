namespace Quartz.Hangfire.Listeners;

public class CleanupTriggerJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var scheduler = context.Scheduler;
        string? triggerKeyName = context.MergedJobDataMap.GetString("TriggerKey");
        if (string.IsNullOrWhiteSpace(triggerKeyName))
        {
            return;
        }
        var triggerKey = new TriggerKey(triggerKeyName);
        while (true)
        {
            var trigger = await scheduler.GetTrigger(triggerKey, context.CancellationToken);
            if (trigger is null)
            {
                return;
            }
            await scheduler.UnscheduleJob(trigger.Key, context.CancellationToken);
            trigger.JobDataMap.TryGetString("NextTrigger", out string? nextKey);
            if (string.IsNullOrWhiteSpace(nextKey))
            {
                return;
            }
            triggerKey = new TriggerKey(nextKey);
        }
    }
}
