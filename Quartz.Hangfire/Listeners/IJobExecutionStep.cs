namespace Quartz.Hangfire.Listeners;

/// <summary>
/// Represents a step in the job execution pipeline.
/// Implementations of this interface can be chained together to add custom logic before and after job execution.
/// </summary>
public interface IJobExecutionStep
{
    /// <summary>
    /// Called when a job is about to be executed.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain. Call this to proceed with the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnExecuting(
        IJobExecutionContext context,
        CancellationToken cancellationToken,
        Func<Task> next);

    /// <summary>
    /// Called when a job has been executed.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    /// <param name="exception">The exception that occurred during job execution, if any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next step in the execution chain. Call this to proceed with the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnExecuted(
        IJobExecutionContext context,
        JobExecutionException? exception,
        CancellationToken cancellationToken,
        Func<Task> next);
}
