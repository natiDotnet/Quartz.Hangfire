using Hangfire;
using Microsoft.Extensions.Logging;

namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A job execution step that automatically retries a job on failure.
/// </summary>
public sealed class AutomaticRetry(ILogger<AutomaticRetry> logger) : IJobExecutionStep
{
    private const string RetryCount = "RetryCount";
    private const string RetryKey = "Retry";

    /// <summary>
    /// Called when a job is about to be executed. This implementation does nothing.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    /// <returns>A completed task.</returns>
    public Task OnExecuting(
        IJobExecutionContext context,
        CancellationToken cancellationToken,
        Func<Task> next)
        => Task.CompletedTask;

    /// <summary>
    /// Called when a job has been executed.
    /// If the job failed, it reschedules it for a future retry.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="exception">The exception that occurred during job execution, if any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnExecuted(
        IJobExecutionContext context,
        JobExecutionException? exception,
        CancellationToken cancellationToken,
        Func<Task> next)
    {
        if (exception is null)
        {
            return next();
        }

        var trigger = context.Trigger;
        trigger.JobDataMap.TryGetInt(RetryCount, out int retry);
        var retryAttribute = trigger.JobDataMap[RetryKey] as AutomaticRetryAttribute;

        if (retry >= retryAttribute!.Attempts)
        {
            return next();
        }
        logger.LogInformation("Retrying job {Key} try {Retry}", trigger.Key, retry);

        int delaySeconds = ComputeDelay(retry, retryAttribute.DelaysInSeconds);

        var newTrigger = trigger
            .GetTriggerBuilder()
            .UsingJobData(RetryCount, retry + 1)
            .StartAt(DateBuilder.FutureDate(delaySeconds, IntervalUnit.Second))
            .Build();

        return context.Scheduler.RescheduleJob(trigger.Key, newTrigger, cancellationToken);
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
