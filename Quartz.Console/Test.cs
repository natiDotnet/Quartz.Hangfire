using Hangfire;

namespace Quartz.Console;

public class Test
{
    [DisableConcurrentExecution(30)]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete, DelaysInSeconds = [10], Order = 2)]
    public async Task<string> TesterAsync(string name, CancellationToken ct = default)
    {
        await Task.Delay(5000, ct);
        System.Console.WriteLine($"Hello Tester {name}");
        return $"Hello Tester {name}";
    }
    
    public static async Task JobCall(ISchedulerFactory factory, string name, TimeSpan? delay = null)
    {
        JobDataMap jobData = new()
        {
            {
                "name", name
            }
        };
        IJobDetail job = JobBuilder.Create<TestJob>()
            .WithIdentity(Guid.NewGuid().ToString())
            .StoreDurably()
            .UsingJobData(jobData)
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue)
            triggerBuilder.StartAt(DateTimeOffset.UtcNow.Add(delay.Value));
        else
            triggerBuilder.StartNow();

        ITrigger trigger = triggerBuilder.Build();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.ScheduleJob(job, trigger);
    }
}
