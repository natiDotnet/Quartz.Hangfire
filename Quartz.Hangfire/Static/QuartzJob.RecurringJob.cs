using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Storage;
using Quartz.Impl;

namespace Quartz.Hangfire;

public static partial class QuartzJob
{
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        [InstantHandle] Expression<Action> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression, associated with a specific queue.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        [InstantHandle] Expression<Action<T>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type, associated with a specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action<T>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression using a static CRON expression.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        [InstantHandle] Expression<Action> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression using a static CRON expression and specific queue.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type using a static CRON expression.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        [InstantHandle] Expression<Action<T>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type using a static CRON expression and specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action<T>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        [InstantHandle] Expression<Func<Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression, associated with a specific queue.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type, associated with a specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression using a static CRON expression.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        [InstantHandle] Expression<Func<Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression using a static CRON expression and specific queue.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type using a static CRON expression.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type using a static CRON expression and specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task AddOrUpdate<T>(
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options);
    }
    
    /// <summary>
    /// Removes a recurring job if it exists.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier of the recurring job to remove.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains true if the job was found and deleted, false otherwise.</returns>
    private static Task<bool> RemoveIfExists([NotNull] string recurringJobId)
    {
        return InternalDelete(factory: null, scheduler: null, JobKey.Create(recurringJobId));
    }
    
    /// <summary>
    /// Triggers the execution of a recurring job immediately.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier of the recurring job to trigger.</param>
    /// <returns>A task containing the job identifier.</returns>
    private static Task<string> TriggerJob([NotNull] string recurringJobId)
    {
        return TriggerJob(recurringJobId);
    }
}
