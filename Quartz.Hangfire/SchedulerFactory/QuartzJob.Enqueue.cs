using System.Linq.Expressions;
using Hangfire.Common;

namespace Quartz.Hangfire;

/// <summary>
/// Provides extension methods for <see cref="ISchedulerFactory"/> to enqueue background jobs.
/// </summary>
public static partial class QuartzJob
{
    /// <summary>
    /// Schedules a background job to be executed as soon as possible.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="methodCall">The method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Action> methodCall)
    {
        return InternalEnqueue(Job.FromExpression(methodCall), factory);
    }
    
    /// <summary>
    /// Schedules a background job on a specific queue to be executed as soon as possible.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="queue">The name of the queue to place the job on.</param>
    /// <param name="methodCall">The method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Action> methodCall)
    {
        return InternalEnqueue(Job.FromExpression(methodCall, queue), factory);
    }
    
    /// <summary>
    /// Schedules an asynchronous background job to be executed as soon as possible.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="methodCall">The asynchronous method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, Expression<Func<Task>> methodCall)
    {
        return InternalEnqueue(Job.FromExpression(methodCall), factory);
    }
    
    /// <summary>
    /// Schedules an asynchronous background job on a specific queue to be executed as soon as possible.
    /// </summary>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="queue">The name of the queue to place the job on.</param>
    /// <param name="methodCall">The asynchronous method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue(this ISchedulerFactory factory, string queue, Expression<Func<Task>> methodCall)
    {
        return InternalEnqueue(Job.FromExpression(methodCall, queue), factory);
    }
    
    /// <summary>
    /// Schedules a background job for a specific service type to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="T">The type of the service to be resolved from the container.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="methodCall">The method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Action<T>> methodCall) where T : notnull
    {
        return InternalEnqueue(Job.FromExpression(methodCall), factory);
    }
    
    /// <summary>
    /// Schedules a background job for a specific service type on a specific queue to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="T">The type of the service to be resolved from the container.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="queue">The name of the queue to place the job on.</param>
    /// <param name="methodCall">The method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Action<T>> methodCall) where T : notnull
    {
        return InternalEnqueue(Job.FromExpression(methodCall, queue), factory);
    }
    
    /// <summary>
    /// Schedules an asynchronous background job for a specific service type to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="T">The type of the service to be resolved from the container.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="methodCall">The asynchronous method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        return InternalEnqueue(Job.FromExpression(methodCall), factory);
    }
    
    /// <summary>
    /// Schedules an asynchronous background job for a specific service type on a specific queue to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="T">The type of the service to be resolved from the container.</typeparam>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="queue">The name of the queue to place the job on.</param>
    /// <param name="methodCall">The asynchronous method call expression representing the job to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, returning the <see cref="TriggerKey"/> of the enqueued job.</returns>
    public static Task<TriggerKey> Enqueue<T>(this ISchedulerFactory factory, string queue, Expression<Func<T, Task>> methodCall) where T : notnull
    {
        return InternalEnqueue(Job.FromExpression(methodCall, queue), factory);
    }
    
}