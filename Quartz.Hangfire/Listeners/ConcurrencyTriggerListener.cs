using System.Collections.Concurrent;
using Hangfire;

namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A trigger listener that enforces concurrency controls for Quartz jobs.
/// It prevents concurrent execution of jobs that share a common trigger name prefix,
/// effectively serializing their execution.
/// </summary>
public sealed class ConcurrencyTriggerListener : ITriggerListener
{
    private readonly ConcurrentDictionary<TriggerKey, byte> _running = new();

    /// <summary>
    /// Gets the name of the trigger listener.
    /// </summary>
    public string Name => "ConcurrencyTriggerListener";

    /// <summary>
    /// Called by the scheduler when a trigger has fired, and it's about to be executed.
    /// </summary>
    /// <param name="trigger">The trigger that has fired.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TriggerFired(
        ITrigger trigger,
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines whether the job execution should be vetoed based on concurrency rules.
    /// </summary>
    /// <param name="trigger">The trigger associated with the job.</param>
    /// <param name="context">The execution context containing job details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the job execution should be vetoed (prevented) because another instance is running;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks if the job is currently being retried within the allowed attempt limit.
    /// If not in a valid retry state, it checks for other currently executing jobs with a matching
    /// trigger name prefix (excluding the unique identifier suffix) to ensure serial execution.
    /// </remarks>
    public async Task<bool> VetoJobExecution(
        ITrigger trigger,
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        trigger.JobDataMap.TryGetInt(DisableConcurrentExecution.RetryCount, out int retryCount);
        if (trigger.JobDataMap[DisableConcurrentExecution.ConcurrentKey] is AutomaticRetryAttribute retryAttribute &&
            retryCount < retryAttribute.Attempts)
            return false;
        string prefix = context.Trigger.Key.Name[..^37];
        // Check if any currently executing job has a trigger starting with prefix
        IReadOnlyCollection<IJobExecutionContext> executingJobs = await context.Scheduler.GetCurrentlyExecutingJobs(cancellationToken);
        return executingJobs.Any(j => j.Trigger.Key.Name.StartsWith(prefix));
    }

    /// <summary>
    /// Called by the scheduler when a trigger has completed its execution.
    /// </summary>
    /// <param name="trigger">The trigger that completed.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="instruction">The instruction code indicating the result of the execution.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TriggerComplete(
        ITrigger trigger,
        IJobExecutionContext context,
        SchedulerInstruction instruction,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by the scheduler when a trigger has misfired.
    /// </summary>
    /// <param name="trigger">The trigger that misfired.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TriggerMisfired(
        ITrigger trigger,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
