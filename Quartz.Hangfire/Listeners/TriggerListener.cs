namespace Quartz.Hangfire.Listeners;

public class LoggingTriggerListener : ITriggerListener
{
    public string Name => "LoggingTriggerListener";

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Trigger fired: {trigger.Key}");
        return Task.CompletedTask;
    }

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false); // allow execution
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Trigger misfired: {trigger.Key}");
        return Task.CompletedTask;
    }

    public Task TriggerComplete(
        ITrigger trigger,
        IJobExecutionContext context,
        SchedulerInstruction triggerInstructionCode,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Trigger complete: {trigger.Key}");
        return Task.CompletedTask;
    }
}

