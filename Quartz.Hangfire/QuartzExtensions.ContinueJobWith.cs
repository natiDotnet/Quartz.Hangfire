using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Quartz.Hangfire.Queue;

namespace Quartz.Hangfire;

/// <summary>
/// Provides extension methods for <see cref="ISchedulerFactory"/> to continue a job with another job.
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="job">The continuation job to execute.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to. Defaults to "default".</param>
    /// <param name="options">The continuation options. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/></param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger was not found.
    /// </returns>
    private static async Task<bool> InternalContinueJobWith(
        ISchedulerFactory factory,
        Job job,
        TriggerKey parentTriggerKey,
        string queue = Default,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        IScheduler scheduler = await factory.GetScheduler();
        ITrigger? parentTrigger = await scheduler.GetTrigger(parentTriggerKey);
        if (parentTrigger is null)
        {
            return false;
        }
        string jobName = $"{queue}_{Guid.NewGuid()}-trigger";
        var nextTrigger = parentTrigger.GetTriggerBuilder()
            .WithIdentity(jobName)
            .ForJob(parentTrigger.JobKey)
            .UsingJobData(new JobDataMap { { "Data", InvocationData.SerializeJob(job) } })
            .WithPriority(QuartzQueues.GetPriority(queue))
            .StartAt(DateTime.MaxValue)
            .Build();

        await scheduler.ScheduleJob(nextTrigger);
        parentTrigger.JobDataMap["NextTrigger"] = jobName;
        parentTrigger.JobDataMap["Options"] = (int)options;
        await scheduler.RescheduleJob(parentTriggerKey, parentTrigger);
        return true;
    }

    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <param name="options">The continuation options. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/></param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static async Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static async Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }

    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Creates a continuation job in a specific queue that will be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to place the continuation job in.</param>
    /// <param name="methodCall">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns <c>true</c> if the continuation was successfully scheduled; otherwise, <c>false</c>.</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options);
    }
}
