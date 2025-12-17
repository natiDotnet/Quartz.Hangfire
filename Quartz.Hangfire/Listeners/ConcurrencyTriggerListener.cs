using System.Collections.Concurrent;
using Hangfire;

namespace Quartz.Hangfire.Listeners;

public sealed class ConcurrencyTriggerListener : ITriggerListener
{
    private readonly ConcurrentDictionary<TriggerKey, byte> _running = new();

    public string Name => "ConcurrencyTriggerListener";

    public Task TriggerFired(
        ITrigger trigger,
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

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

    public Task TriggerComplete(
        ITrigger trigger,
        IJobExecutionContext context,
        SchedulerInstruction instruction,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerMisfired(
        ITrigger trigger,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
