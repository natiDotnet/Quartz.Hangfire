namespace Quartz.Hangfire.Listeners;

public interface IJobExecutionStep
{
    Task OnExecuting(
        IJobExecutionContext context,
        CancellationToken cancellationToken,
        Func<Task> next);

    Task OnExecuted(
        IJobExecutionContext context,
        JobExecutionException? exception,
        CancellationToken cancellationToken,
        Func<Task> next);
}
