namespace Quartz.Hangfire.Listeners;

public sealed class MySchedulerListener : ISchedulerListener
{
    public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task SchedulerStarted(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Quartz scheduler started");
        return Task.CompletedTask;
    }
    public Task SchedulerStarting(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task SchedulerShutdown(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    public Task SchedulingDataCleared(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

