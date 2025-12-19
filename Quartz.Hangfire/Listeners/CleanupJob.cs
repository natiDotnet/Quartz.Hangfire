namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A Quartz job responsible for cleaning up a chain of triggers.
/// </summary>
/// <remarks>
/// This job is designed to iteratively unschedule a sequence of linked triggers.
/// It starts with a trigger specified in the job data and follows the "NextTrigger" chain
/// to ensure all related triggers are removed from the scheduler.
/// </remarks>
public class CleanupTriggerJob : IJob
{
    /// <summary>
    /// Executes the cleanup logic asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing the scheduler reference and merged job data map.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// The method retrieves the starting trigger key from <see cref="IJobExecutionContext.MergedJobDataMap"/> using the key "TriggerKey".
    /// It then enters a loop to:
    /// <list type="number">
    /// <item>Retrieve the trigger from the scheduler.</item>
    /// <item>Unschedule the job associated with the trigger.</item>
    /// <item>Check for a "NextTrigger" key in the trigger's data map to continue the chain.</item>
    /// </list>
    /// The process stops if the initial key is missing, a trigger is not found, or the chain ends.
    /// </remarks>
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
