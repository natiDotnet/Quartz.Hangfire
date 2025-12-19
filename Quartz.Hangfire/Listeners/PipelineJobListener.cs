namespace Quartz.Hangfire.Listeners;

/// <summary>
/// A Quartz job listener that executes a pipeline of <see cref="IJobExecutionStep"/>s 
/// before and after job execution.
/// </summary>
/// <remarks>
/// This listener orchestrates the execution of registered steps in a middleware-like pipeline,
/// allowing cross-cutting concerns to be applied to Quartz jobs.
/// </remarks>
/// <param name="steps">The collection of execution steps to include in the pipeline.</param>
public sealed class PipelineJobListener(IEnumerable<IJobExecutionStep> steps) : IJobListener
{
    private readonly IReadOnlyList<IJobExecutionStep> _steps = steps.ToList();

    /// <inheritdoc />
    public string Name => "job-pipeline";

    /// <summary>
    /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/> is about to be executed.
    /// </summary>
    /// <remarks>
    /// Executes the <see cref="IJobExecutionStep.OnExecuting"/> method of each registered step in the pipeline.
    /// </remarks>
    /// <param name="context">The execution context containing information about the job and trigger.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task JobToBeExecuted(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        int index = 0;
        // Recursive "next" delegate
        Func<Task> next = null!;
        next = async () =>
        {
            if (index < _steps.Count)
            {
                IJobExecutionStep step = _steps[index++];
                await step.OnExecuting(context, cancellationToken, next);
            }
        };

        await next();
    }

    /// <summary>
    /// Called by the <see cref="IScheduler"/> after a <see cref="IJobDetail"/> has been executed.
    /// </summary>
    /// <remarks>
    /// Executes the <see cref="IJobExecutionStep.OnExecuted"/> method of each registered step in the pipeline.
    /// </remarks>
    /// <param name="context">The execution context containing information about the job and trigger.</param>
    /// <param name="jobException">The exception thrown by the job, if any; otherwise null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        int index = 0;
        // Recursive "next" delegate
        Func<Task> next = null!;
        next = async () =>
        {
            if (index < _steps.Count)
            {
                var step = _steps[index++];
                await step.OnExecuted(context, jobException, cancellationToken, next);
            }
        };

        await next();
    }

    /// <summary>
    /// Called by the <see cref="IScheduler"/> when a <see cref="IJobDetail"/> execution is vetoed.
    /// </summary>
    /// <remarks>
    /// This implementation does not perform any actions when a job is vetoed.
    /// </remarks>
    /// <param name="context">The execution context containing information about the job and trigger.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
    public Task JobExecutionVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
