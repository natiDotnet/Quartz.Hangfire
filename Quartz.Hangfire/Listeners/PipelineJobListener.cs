namespace Quartz.Hangfire.Listeners;

public sealed class PipelineJobListener(IEnumerable<IJobExecutionStep> steps) : IJobListener
{
    private readonly IReadOnlyList<IJobExecutionStep> _steps = steps.ToList();

    public string Name => "job-pipeline";

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

    public Task JobExecutionVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

