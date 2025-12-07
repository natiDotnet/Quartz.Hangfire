using System.Linq.Expressions;
using Hangfire.Common;

namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Schedules a job to be executed after a specified delay
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Action> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, delay: delay);
    }
    
    /// <summary>
    /// Schedules a job to be executed after a specified delay on a specific queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Action> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    /// <summary>
    /// Schedules an asynchronous job to be executed after a specified delay
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, delay: delay);
    }
    
    /// <summary>
    /// Schedules an asynchronous job to be executed after a specified delay on a specific queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    /// <summary>
    /// Schedules a job to be executed at a specific time
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Action> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules a job to be executed at a specific time on a specific queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Action> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules an asynchronous job to be executed at a specific time
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules an asynchronous job to be executed at a specific time on a specific queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules a generic job to be executed after a specified delay
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, delay: delay);
    }
    
    /// <summary>
    /// Schedules a generic job to be executed after a specified delay on a specific queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    /// <summary>
    /// Schedules a generic asynchronous job to be executed after a specified delay
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, delay: delay);
    }
    
    /// <summary>
    /// Schedules a generic asynchronous job to be executed after a specified delay on a specific queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="delay">The delay before executing the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall, TimeSpan delay)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, delay);
    }
    
    /// <summary>
    /// Schedules a generic job to be executed at a specific time
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules a generic job to be executed at a specific time on a specific queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules a generic asynchronous job to be executed at a specific time
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, enqueueAt: enqueueAt);
    }
    
    /// <summary>
    /// Schedules a generic asynchronous job to be executed at a specific time on a specific queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the job</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="queue">The queue to enqueue the job on</param>
    /// <param name="methodCall">The async method to execute</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>The trigger key for the scheduled job</returns>
    public static Task<TriggerKey> Schedule<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
        where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue, enqueueAt: enqueueAt);
    }
    
}