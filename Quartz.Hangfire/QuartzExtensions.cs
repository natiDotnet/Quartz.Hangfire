using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Quartz.Hangfire.Queue;
using Quartz.Impl;

namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// The default queue name if none is specified
    /// </summary>
    private const string Default = "default";

    /// <summary>
    /// Enqueues a job for execution using Quartz scheduler
    /// </summary>
    /// <param name="factory">The scheduler factory used to get the Quartz scheduler instance</param>
    /// <param name="job">The Hangfire job to be executed</param>
    /// <param name="delay">Optional delay before the job starts executing</param>
    /// <param name="enqueueAt">Optional specific date/time when the job should start executing</param>
    /// <param name="scheduler">The scheduler used to get the Quartz scheduler instance</param>
    /// <returns>The TriggerKey of the scheduled job</returns>
    internal static async Task<TriggerKey> InternalEnqueue(
        Job job,
        ISchedulerFactory? factory = null,
        TimeSpan? delay = null,
        DateTimeOffset? enqueueAt = null,
        IScheduler? scheduler = null)
    {
        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity($"{job}-{Guid.NewGuid()}")
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue || enqueueAt.HasValue)
            triggerBuilder.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            triggerBuilder.StartNow();
        
        var filters = JobFilterProviders.Providers.GetFilters(job).ToList();
        var retry = filters
            .Select(f => f.Instance)
            .OfType<AutomaticRetryAttribute>()
            .First();

        var disableConcurrentExecutionAttribute = filters
            .Select(f => f.Instance)
            .OfType<DisableConcurrentExecutionAttribute>()
            .FirstOrDefault();
        AutomaticRetryAttribute? concurrentRetry = null;
        if (disableConcurrentExecutionAttribute is not null)
        {
            concurrentRetry = new AutomaticRetryAttribute
            {
                Order = disableConcurrentExecutionAttribute.Order
            };
            concurrentRetry.DelaysInSeconds ??= [5, ..Enumerable
                .Repeat(10, disableConcurrentExecutionAttribute.TimeoutSec / 10)
                .ToArray()];
            concurrentRetry.Attempts = concurrentRetry.DelaysInSeconds.Length;
        }
        
        string queue = job.Queue ?? Default;
        int priority = QuartzQueues.GetPriority(queue);

        ITrigger trigger = triggerBuilder
            .ForJob(expressionJob)
            .UsingJobData(new JobDataMap
            {
                { "Data", InvocationData.SerializeJob(job) },
                { "Retry", retry },
                { "Concurrent", concurrentRetry! }
            })
            .WithIdentity($"{job}-{Guid.NewGuid()}")
            .WithPriority(priority)
            .Build();
        scheduler ??= await GetScheduler(factory);
        await scheduler.ScheduleJob(expressionJob, trigger);
        return trigger.Key;
    }

    private static async Task<IScheduler> GetScheduler(ISchedulerFactory? factory = null)
    {
        return factory is not null
            ? await factory.GetScheduler()
            : await StdSchedulerFactory.GetDefaultScheduler();
    }
}
