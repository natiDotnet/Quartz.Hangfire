using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Storage;

namespace Quartz.Hangfire;

public static partial class QuartzJob
{
    /// <summary>
    /// Schedules or updates a recurring job using Quartz.NET with the specified configuration.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="job">The Hangfire job definition to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional settings for the recurring job, such as time zone and misfire handling.</param>
    /// <param name="factory">The scheduler factory to retrieve the scheduler instance.</param>
    /// <param name="scheduler">The scheduler instance to use. If null, one will be retrieved from the factory.</param>
    /// <remarks>
    /// This method creates a Quartz job detail and trigger, serializing the Hangfire job data for execution.
    /// </remarks>
    private static async Task AddOrUpdate(string recurringJobId, Job job, string cronExpression, RecurringJobOptions? options = null, ISchedulerFactory? factory = null, IScheduler? scheduler = null)
    {
        options ??= new RecurringJobOptions();
        IJobDetail expressionJob = JobBuilder.Create<ExpressionJob>()
            .WithIdentity(recurringJobId)
            .UsingJobData(new JobDataMap
            {
                { "Data", InvocationData.SerializeJob(job) }
            })
            .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{recurringJobId}-trigger")
                .WithCronSchedule(cronExpression,
                    builder => MisfireHandle(builder, options))
                .Build();
            scheduler = await GetScheduler(factory, scheduler);
            await scheduler.ScheduleJob(expressionJob, trigger);
    }

    /// <summary>
    /// Configures the misfire handling instruction for the CRON schedule based on the provided options.
    /// </summary>
    /// <param name="builder">The CRON schedule builder to configure.</param>
    /// <param name="options">The options containing the desired misfire handling mode.</param>
    /// <returns>The configured CRON schedule builder.</returns>
    private static void MisfireHandle(CronScheduleBuilder builder, RecurringJobOptions options)
    {
        builder.InTimeZone(options.TimeZone);
        switch (options.MisfireHandling)
        {
            case MisfireHandlingMode.Ignorable:
                builder.WithMisfireHandlingInstructionIgnoreMisfires();
                break;
            case MisfireHandlingMode.Strict:
                builder.WithMisfireHandlingInstructionFireAndProceed();
                break;
            case MisfireHandlingMode.Relaxed:
            default:
                builder.WithMisfireHandlingInstructionDoNothing();
                break;
        }
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Action> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression, associated with a specific queue.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Action<T>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type, associated with a specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action<T>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression using a static CRON expression.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Action> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression using a static CRON expression and specific queue.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type using a static CRON expression.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Action<T>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on a synchronous method call expression on a specific type using a static CRON expression and specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Action<T>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Func<Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression, associated with a specific queue.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type, associated with a specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">A function returning the CRON expression for the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        Func<string> cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression(), options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression using a static CRON expression.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Func<Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression using a static CRON expression and specific queue.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type using a static CRON expression.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Schedules a recurring job based on an asynchronous method call expression on a specific type using a static CRON expression and specific queue.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier for the recurring job.</param>
    /// <param name="queue">The queue name where the job should be enqueued.</param>
    /// <param name="methodCall">The expression representing the asynchronous method to be executed.</param>
    /// <param name="cronExpression">The CRON expression defining the schedule.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task AddOrUpdate<T>(
        this ISchedulerFactory factory,
        string recurringJobId,
        string queue,
        [InstantHandle] Expression<Func<T, Task>> methodCall,
        string cronExpression,
        RecurringJobOptions? options = null)
    {
        var job = Job.FromExpression(methodCall, queue);
        return AddOrUpdate(recurringJobId, job, cronExpression, options, factory);
    }
    
    /// <summary>
    /// Removes a recurring job if it exists.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier of the recurring job to remove.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains true if the job was found and deleted, false otherwise.</returns>
    public static Task<bool> RemoveIfExists(this ISchedulerFactory factory, [NotNull] string recurringJobId)
    {
        return Delete(factory, JobKey.Create(recurringJobId));
    }
    
    /// <summary>
    /// Triggers the execution of a recurring job immediately.
    /// </summary>
    /// <param name="factory">The scheduler factory used to access the Quartz scheduler.</param>
    /// <param name="recurringJobId">The unique identifier of the recurring job to trigger.</param>
    /// <returns>A task containing the job identifier.</returns>
    public static Task<string> TriggerJob(this ISchedulerFactory factory, [NotNull] string recurringJobId)
    {
        return TriggerJob(recurringJobId, factory);
    }

    /// <summary>
    /// Internal helper to trigger a job immediately.
    /// </summary>
    /// <param name="recurringJobId">The unique identifier of the recurring job.</param>
    /// <param name="factory">The scheduler factory.</param>
    /// <param name="scheduler">The scheduler instance.</param>
    /// <returns>A task containing the job identifier.</returns>
    private static async Task<string> TriggerJob(string recurringJobId, ISchedulerFactory? factory = null, IScheduler? scheduler = null)
    {
        scheduler = await GetScheduler(factory, scheduler);
        await scheduler.TriggerJob(JobKey.Create(recurringJobId));
        return recurringJobId;
    }
}
