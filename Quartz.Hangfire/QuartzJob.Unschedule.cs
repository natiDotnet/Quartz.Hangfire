namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzJob
{
    /// <summary>
    /// Internal method to unschedule multiple jobs based on their trigger keys
    /// </summary>
    /// <param name="factory">The scheduler factory instance</param>
    /// <param name="triggerKeys">Array of trigger keys to unschedule</param>
    /// <returns>True if all jobs were successfully unscheduled, false otherwise</returns>
    private static async Task<bool> InternalUnschedule(ISchedulerFactory factory, params TriggerKey[] triggerKeys)
    {
        IScheduler scheduler = await factory.GetScheduler();
        return await scheduler.UnscheduleJobs(triggerKeys);
    }

    /// <summary>
    /// Unschedules a job based on its trigger key
    /// </summary>
    /// <param name="factory">The scheduler factory instance</param>
    /// <param name="triggerKey">The trigger key of the job to unschedule</param>
    /// <returns>True if the job was successfully unscheduled, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when triggerKey is null</exception>
    public static Task<bool> UnscheduleJobs(this ISchedulerFactory factory, TriggerKey triggerKey)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalUnschedule(factory, triggerKey);
    }
}