using System.Linq.Expressions;
using Hangfire.Common;

namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Enqueues a job for immediate execution using the specified method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="methodCall">The method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Action> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    /// <summary>
    /// Enqueues a job for immediate execution using the specified queue and method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="queue">The queue name to use.</param>
    /// <param name="methodCall">The method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Action> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    /// <summary>
    /// Enqueues an asynchronous job for immediate execution using the specified method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="methodCall">The asynchronous method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Func<Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    /// <summary>
    /// Enqueues an asynchronous job for immediate execution using the specified queue and method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="queue">The queue name to use.</param>
    /// <param name="methodCall">The asynchronous method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall)
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    /// <summary>
    /// Enqueues a generic job for immediate execution using the specified method call expression.
    /// </summary>
    /// <typeparam name="T">The type parameter for the job.</typeparam>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="methodCall">The method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    /// <summary>
    /// Enqueues a generic job for immediate execution using the specified queue and method call expression.
    /// </summary>
    /// <typeparam name="T">The type parameter for the job.</typeparam>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="queue">The queue name to use.</param>
    /// <param name="methodCall">The method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
    /// <summary>
    /// Enqueues a generic asynchronous job for immediate execution using the specified method call expression.
    /// </summary>
    /// <typeparam name="T">The type parameter for the job.</typeparam>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="methodCall">The asynchronous method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job);
    }
    
    /// <summary>
    /// Enqueues a generic asynchronous job for immediate execution using the specified queue and method call expression.
    /// </summary>
    /// <typeparam name="T">The type parameter for the job.</typeparam>
    /// <param name="factory">The scheduler factory instance.</param>
    /// <param name="queue">The queue name to use.</param>
    /// <param name="methodCall">The asynchronous method call expression to execute.</param>
    /// <returns>A task containing the trigger key for the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var job = Job.FromExpression(methodCall);
        return InternalEnqueue(factory, job, queue);
    }
    
}