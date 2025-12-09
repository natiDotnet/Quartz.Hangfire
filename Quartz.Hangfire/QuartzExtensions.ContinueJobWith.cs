using System.Linq.Expressions;
using Hangfire.Common;
using Hangfire.Storage;

namespace Quartz.Hangfire;

/// <summary>
/// Extension methods for Quartz scheduler operations
/// </summary>
public static partial class QuartzExtensions
{
    /// <summary>
    /// Internal method to continue a job with another job
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="job">The job to continue with</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">Optional queue name</param>
    /// <returns>True if successful, false if parent job not found</returns>
    private static async Task<bool> InternalContinueJobWith(
        ISchedulerFactory factory,
        Job job,
        JobKey parentJobKey,
        string? queue = null)
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
            .StoreDurably(true)
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
    
    /// <summary>
    /// Continues a job with a synchronous action
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static async Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Action> methodCall)
    {
        return await InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    /// <summary>
    /// Continues a job with a synchronous action
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static async Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentJobKey,
        Expression<Action> methodCall)
    {
        return await InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey));
    }
    
    /// <summary>
    /// Continues a job with a generic synchronous action
    /// </summary>
    /// <typeparam name="T">The type parameter for the action</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    /// <summary>
    /// Continues a job with a generic synchronous action
    /// </summary>
    /// <typeparam name="T">The type parameter for the action</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentJobKey,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey));
    }
    
    /// <summary>
    /// Continues a job with a synchronous action in a specified queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Action> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    /// <summary>
    /// Continues a job with a synchronous action in a specified queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentJobKey,
        string queue,
        Expression<Action> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey), queue);
    }
    
    /// <summary>
    /// Continues a job with a generic synchronous action in a specified queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the action</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    /// <summary>
    /// Continues a job with a generic synchronous action in a specified queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the action</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The action to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentJobKey,
        string queue,
        Expression<Action<T>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey), queue);
    }

    /// <summary>
    /// Continues a job with an asynchronous function
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    /// <summary>
    /// Continues a job with an asynchronous function
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentJobKey,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey));
    }
    
    /// <summary>
    /// Continues a job with an asynchronous function in a specified queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    /// <summary>
    /// Continues a job with an asynchronous function in a specified queue
    /// </summary>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith(
        this ISchedulerFactory factory,
        string parentJobKey,
        string queue,
        Expression<Func<Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey), queue);
    }
    
    /// <summary>
    /// Continues a job with a generic asynchronous function
    /// </summary>
    /// <typeparam name="T">The type parameter for the function</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey);
    }
    
    /// <summary>
    /// Continues a job with a generic asynchronous function
    /// </summary>
    /// <typeparam name="T">The type parameter for the function</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentJobKey,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey));
    }
    
    /// <summary>
    /// Continues a job with a generic asynchronous function in a specified queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the function</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        JobKey parentJobKey,
        string queue,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), parentJobKey, queue);
    }
    
    /// <summary>
    /// Continues a job with a generic asynchronous function in a specified queue
    /// </summary>
    /// <typeparam name="T">The type parameter for the function</typeparam>
    /// <param name="factory">The scheduler factory</param>
    /// <param name="parentJobKey">The key of the parent job</param>
    /// <param name="queue">The queue name</param>
    /// <param name="methodCall">The async function to execute</param>
    /// <returns>True if successful, false if parent job not found</returns>
    public static Task<bool> ContinueJobWith<T>(
        this ISchedulerFactory factory,
        string parentJobKey,
        string queue,
        Expression<Func<T, Task>> methodCall)
    {
        return InternalContinueJobWith(factory, Job.FromExpression(methodCall), JobKey.Create(parentJobKey), queue);
    }
}