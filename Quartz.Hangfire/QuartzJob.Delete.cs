namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzJob
{
    /// <summary>
    /// Internal method to delete one or more jobs from the scheduler
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="jobKeys">One or more job keys to delete</param>
    /// <returns>True if all jobs were successfully deleted</returns>
    private static async Task<bool> InternalDelete(ISchedulerFactory? factory = null, IScheduler? scheduler = null, params JobKey[] jobKeys)
    {
        scheduler = await GetScheduler(factory, scheduler);
        if (jobKeys.Length == 1)
        {
            return await scheduler.DeleteJob(jobKeys[0]);
        }
        return await scheduler.DeleteJobs(jobKeys.ToList());
    }

    /// <summary>
    /// Deletes a single job from the scheduler
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="jobKey">The key of the job to delete</param>
    /// <returns>True if the job was successfully deleted</returns>
    /// <exception cref="ArgumentNullException">Thrown when jobKey is null</exception>
    public static Task<bool> Delete(this ISchedulerFactory factory, JobKey jobKey)
    {
        ArgumentNullException.ThrowIfNull(jobKey);
        return InternalDelete(factory, scheduler: null, jobKey);
    }
    
    /// <summary>
    /// Deletes multiple jobs from the scheduler
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="jobKeys">Array of job keys to delete</param>
    /// <returns>True if all jobs were successfully deleted</returns>
    /// <exception cref="ArgumentNullException">Thrown when jobKeys is null</exception>
    public static Task<bool> Delete(this ISchedulerFactory factory, JobKey[] jobKeys)
    {
        ArgumentNullException.ThrowIfNull(jobKeys);
        return InternalDelete(factory, scheduler: null, jobKeys);
    }
    
}