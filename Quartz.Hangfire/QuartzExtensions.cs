using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Hangfire.Listeners;
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
    /// <param name="queue">Optional queue name. If not specified, a new GUID will be used</param>
    /// <param name="delay">Optional delay before the job starts executing</param>
    /// <param name="enqueueAt">Optional specific date/time when the job should start executing</param>
    /// <param name="scheduler">The scheduler used to get the Quartz scheduler instance</param>
    /// <returns>The TriggerKey of the scheduled job</returns>
    internal static async Task<TriggerKey> InternalEnqueue(
        Job job,
        ISchedulerFactory? factory = null,
        string queue = Default,
        TimeSpan? delay = null,
        DateTimeOffset? enqueueAt = null,
        IScheduler? scheduler = null)
    {
        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity($"{queue}_{Guid.NewGuid()}")
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue || enqueueAt.HasValue)
            triggerBuilder.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            triggerBuilder.StartNow();
        
        int priority = QuartzQueues.GetPriority(queue);

        var filters = JobFilterProviders.Providers.GetFilters(job);
        var retry = filters
            .Select(f => f.Instance)
            .OfType<AutomaticRetryAttribute>()
            .First();

        ITrigger trigger = triggerBuilder
            .ForJob(expressionJob)
            .UsingJobData(new JobDataMap
            {
                { "Data", InvocationData.SerializeJob(job) },
                { "Retry", retry }
            })
            .WithIdentity($"{queue}_{Guid.NewGuid()}-trigger")
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