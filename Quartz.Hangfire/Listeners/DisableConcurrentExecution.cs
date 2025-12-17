using Hangfire;
using Microsoft.Extensions.Logging;

namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A job execution step that prevents concurrent execution of jobs with the same prefix.
/// If a job with the same prefix is already running, it reschedules the current job to run later.
/// </summary>
/// <param name="logger">The logger.</param>
public sealed class DisableConcurrentExecution(ILogger<AutomaticRetry> logger) : IJobExecutionStep
{
    /// <summary>
    /// The key for the retry count in the job data map.
    /// </summary>
    internal const string RetryCount = "ConcurrentRetryCount";
    
    /// <summary>
    /// The key for the concurrent execution attribute in the job data map.
    /// </summary>
    internal const string ConcurrentKey = "Concurrent";

    /// <summary>
    /// Called when a job is about to be executed.
    /// Checks if another job with the same prefix is already running. If so, it reschedules the current job and prevents its execution.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    public async Task OnExecuting(
        IJobExecutionContext context,
        CancellationToken cancellationToken,
        Func<Task> next)
    {
        ITrigger trigger = context.Trigger;
        trigger.JobDataMap.TryGetInt(RetryCount, out int retryCount);
        if (trigger.JobDataMap[ConcurrentKey] is not AutomaticRetryAttribute retryAttribute ||
            retryCount >= retryAttribute.Attempts)
        {
            await next();
            return;
        }
        var prefix = context.Trigger.Key.Name[..^37];
        // Check if any currently executing job has a trigger starting with prefix
        var executingJobs = await context.Scheduler.GetCurrentlyExecutingJobs(cancellationToken);
        bool conflict = executingJobs.Any(j => j.Trigger.Key.Name.StartsWith(prefix));

        if (!conflict)
        {
            await next();
            return;
        }

        logger.LogInformation("Concurrent Retrying job {Key} try {Retry}", trigger.Key, retryCount);

        int delaySeconds = ComputeDelay(retryCount, retryAttribute.DelaysInSeconds);

        ITrigger newTrigger = trigger
            .GetTriggerBuilder()
            .UsingJobData(RetryCount, retryCount + 1)
            .StartAt(DateBuilder.FutureDate(delaySeconds, IntervalUnit.Second))
            .Build();

        await context.Scheduler.RescheduleJob(trigger.Key, newTrigger, cancellationToken);
        throw new JobExecutionException("job already running...")
        {
            RefireImmediately = false
        };
    }

    /// <summary>
    /// Called when a job has been executed. This implementation does nothing.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="exception">The exception that occurred during job execution, if any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    /// <returns>A completed task.</returns>
    public Task OnExecuted(
        IJobExecutionContext context,
        JobExecutionException? exception,
        CancellationToken cancellationToken,
        Func<Task> next)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Computes the delay for the next retry.
    /// </summary>
    /// <param name="retry">The current retry count.</param>
    /// <param name="delays">An optional array of delays in seconds. If not provided, an exponential backoff is used.</param>
    /// <returns>The delay in seconds.</returns>
    private static int ComputeDelay(int retry, int[]? delays)
        => delays switch
        {
            { Length: > 0 } when retry < delays.Length => delays[retry],
            { Length: > 0 } => delays[^1],           // retry >= Length
            _ => (int)Math.Pow(2, retry)  // null or empty array
        };
}
