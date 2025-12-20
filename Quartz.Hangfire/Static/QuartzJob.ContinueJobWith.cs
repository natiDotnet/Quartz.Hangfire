using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;

namespace Quartz.Hangfire;

/// <summary>
/// Provides static methods for creating Hangfire continuation jobs that execute after a Quartz job.
/// </summary>
public static partial class QuartzJob
{
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static async Task<bool> ContinueJobWith(
        TriggerKey parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static async Task<bool> ContinueJobWith(
        string parentTriggerKey,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return await InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        TriggerKey parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        string parentTriggerKey,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        string parentTriggerKey,
        string queue,
        Expression<Action> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        string parentTriggerKey,
        string queue,
        Expression<Action<T>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }

    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        TriggerKey parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        string parentTriggerKey,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith(
        string parentTriggerKey,
        string queue,
        Expression<Func<Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        TriggerKey parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        string parentTriggerKey,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The trigger key of the parent job.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        TriggerKey parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), parentTriggerKey, queue, options: options);
    }
    
    /// <summary>
    /// Schedules a continuation job in a specific queue to be executed after the parent job completes.
    /// </summary>
    /// <typeparam name="T">The type of the service whose method will be invoked.</typeparam>
    /// <param name="parentTriggerKey">The string representation of the parent job's trigger key.</param>
    /// <param name="queue">The queue to enqueue the continuation job to.</param>
    /// <param name="methodCall">The method call expression representing the job to execute.</param>
    /// <param name="options">The options for the continuation job. Defaults to <see cref="JobContinuationOptions.OnlyOnSucceededState"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the continuation was successfully scheduled;
    /// otherwise, <c>false</c> if the parent trigger could not be found.
    /// </returns>
    private static Task<bool> ContinueJobWith<T>(
        string parentTriggerKey,
        string queue,
        Expression<Func<T, Task>> methodCall,
        JobContinuationOptions options = JobContinuationOptions.OnlyOnSucceededState)
    {
        return InternalContinueJobWith(Job.FromExpression(methodCall), new TriggerKey(parentTriggerKey), queue, options);
    }
}
