using System.Linq.Expressions;
using Hangfire.Common;
using Hangfire.Storage;

namespace Quartz.Hangfire;

public static class QuartzExtensions
{
    public static async Task JobCall(this ISchedulerFactory factory, string name, TimeSpan? delay = null)
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
    private static async Task<TriggerKey> InternalEnqueue(ISchedulerFactory factory, Job job, string? queue = null, TimeSpan? delay = null, DateTimeOffset? enqueueAt = null)
    {
        JobDataMap jobData = new()
        {
            {
                "Data", InvocationData.SerializeJob(job)
            }
        };

        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity(queue ?? Guid.NewGuid().ToString())
            .UsingJobData(jobData)
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue || enqueueAt.HasValue)
            triggerBuilder.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            triggerBuilder.StartNow();

        ITrigger trigger = triggerBuilder
            .WithIdentity($"{queue ?? Guid.NewGuid().ToString()}-trigger")
            .Build();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.ScheduleJob(expressionJob, trigger);
        return trigger.Key;
    }
    
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Action> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Action> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Func<Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Action> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, delay);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Action> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, delay);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Action> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Action> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, delay);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, delay);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, null, null, enqueueAt);
    }
    
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, null, enqueueAt);
    }
    
    private static async Task<bool> InternalDelete(ISchedulerFactory factory, params JobKey[] jobKeys)
    {
        IScheduler scheduler = await factory.GetScheduler();
        if (jobKeys.Length == 1)
        {
            return await scheduler.DeleteJob(jobKeys[0]);
        }
        return await scheduler.DeleteJobs(jobKeys.ToList());
    }

    private static async Task<bool> InternalUnschedule(ISchedulerFactory factory, params TriggerKey[] triggerKeys)
    {
        IScheduler scheduler = await factory.GetScheduler();
        return await scheduler.UnscheduleJobs(triggerKeys);
    }
    
    private static async Task<DateTimeOffset?> InternalReschedule(ISchedulerFactory factory, TriggerKey triggerKey, TimeSpan? delay = null, DateTimeOffset? enqueueAt = null)
    {
        IScheduler scheduler = await factory.GetScheduler();
        TriggerBuilder trigger = TriggerBuilder.Create().WithIdentity(triggerKey);
        if (delay.HasValue || enqueueAt.HasValue)
            trigger.StartAt(enqueueAt ?? DateTimeOffset.UtcNow.Add(delay!.Value));
        else
            trigger.StartNow();
        return await scheduler.RescheduleJob(triggerKey, trigger.Build());
    }
    
    
    public static Task<bool> Delete(this ISchedulerFactory factory, JobKey jobKey)
    {
        ArgumentNullException.ThrowIfNull(jobKey);
        return InternalDelete(factory, jobKey);
    }
    
    public static Task<bool> Delete(this ISchedulerFactory factory, JobKey[] jobKeys)
    {
        ArgumentNullException.ThrowIfNull(jobKeys);
        return InternalDelete(factory, jobKeys);
    }
    
    public static Task<bool> UnscheduleJobs(this ISchedulerFactory factory, TriggerKey triggerKey)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalUnschedule(factory, triggerKey);
    }
    
    public static Task<DateTimeOffset?> Reschedule(this ISchedulerFactory factory, TriggerKey triggerKey, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalReschedule(factory, triggerKey, delay);
    }
    
    public static Task<DateTimeOffset?> Reschedule(this ISchedulerFactory factory, TriggerKey triggerKey, DateTimeOffset enqueueAt)
    {
        ArgumentNullException.ThrowIfNull(triggerKey);
        return InternalReschedule(factory, triggerKey, enqueueAt: enqueueAt);
    }
    private static async Task<bool> InternalContinueJobWith(ISchedulerFactory factory, Job job, JobKey parentJobKey, string? queue = null)
    {
        JobDataMap jobData = new()
        {
            {
                "Data", InvocationData.SerializeJob(job)
            }
        };
        var jobName = queue ?? Guid.NewGuid().ToString();
        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity(jobName)
            .StoreDurably()
            .UsingJobData(jobData)
            .Build();
        
        var scheduler = await factory.GetScheduler();
        // add the new job
        await scheduler.AddJob(expressionJob, true);
        
        // update the parent job
        IJobDetail? parentJob = await scheduler.GetJobDetail(parentJobKey);
        if (parentJob is null)
        {
            return false;
        }
        parentJob.JobDataMap["NextJob"] = jobName;
        if (!parentJob.Durable)
        {
            parentJob = parentJob
                .GetJobBuilder()
                .StoreDurably()
                .Build();
        }
        await scheduler.AddJob(parentJob, replace: true);
        
        return true;
    }
    
    public static Task<bool> ContinueJobWith(this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Action> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    public static Task<bool> ContinueJobWith<T>(this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    public static Task<bool> ContinueJobWith(this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Action> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    public static Task<bool> ContinueJobWith<T>(this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }

    public static Task<bool> ContinueJobWith(this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    public static Task<bool> ContinueJobWith(this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    public static Task<bool> ContinueJobWith<T>(this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    public static Task<bool> ContinueJobWith<T>(this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
}
