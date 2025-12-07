namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Internal method to reschedule a job with the specified trigger key
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="triggerKey">The trigger key identifying the job to reschedule</param>
    /// <param name="delay">Optional delay before the job should run</param>
    /// <param name="enqueueAt">Optional specific time when the job should run</param>
    /// <returns>The next time the job will run, or null if the job has been removed</returns>
    private static async Task<DateTimeOffset?> InternalReschedule(ISchedulerFactory factory, TriggerKey triggerKey, TimeSpan? delay = null, DateTimeOffset? enqueueAt = null)
    {
        TriggerBuilder trigger = TriggerBuilder.Create().WithIdentity(triggerKey);
        if (delay.HasValue || enqueueAt.HasValue)
            trigger.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            trigger.StartNow();
        IScheduler scheduler = await factory.GetScheduler();
        return await scheduler.RescheduleJob(triggerKey, trigger.Build());
    }
    
    /// <summary>
    /// Reschedules a job to run after the specified delay
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="triggerKey">The trigger key identifying the job to reschedule</param>
    /// <param name="delay">The time to wait before running the job</param>
    /// <returns>The next time the job will run, or null if the job has been removed</returns>
    public static Task<DateTimeOffset?> Reschedule(this ISchedulerFactory factory, TriggerKey triggerKey, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalReschedule(factory, triggerKey, delay);
    }
    
    /// <summary>
    /// Reschedules a job to run at the specified time
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="triggerKey">The trigger key identifying the job to reschedule</param>
    /// <param name="enqueueAt">The specific time when the job should run</param>
    /// <returns>The next time the job will run, or null if the job has been removed</returns>
    public static Task<DateTimeOffset?> Reschedule(this ISchedulerFactory factory, TriggerKey triggerKey, DateTimeOffset enqueueAt)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalReschedule(factory, triggerKey, enqueueAt: enqueueAt);
    }
}