using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Quartz.Hangfire.Queue;

namespace Quartz.Hangfire;

/// <summary>
/// Provides extension methods for <see cref="IScheduler"/> to schedule continuation jobs.
/// </summary>
public static partial class QuartzJob
{
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static async Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static async Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        string parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        string parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        string parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        string parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options, scheduler: scheduler);
    }

    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        string parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith(
        this IScheduler scheduler,
        string parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        string parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options, scheduler: scheduler);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/> instance.</param>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    public static Task<bool> ContinueJobWith<T>(
        this IScheduler scheduler,
        string parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options, scheduler: scheduler);
    }
}
