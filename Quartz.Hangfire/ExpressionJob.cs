using System.Reflection;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Hangfire.Queue;

namespace Quartz.Hangfire;

/// <summary>
/// Job implementation that executes expressions using dependency injection
/// </summary>
internal class ExpressionJob(IServiceScopeFactory serviceScopeFactory) : IJob
{
    /// <summary>
    /// Executes the job by invoking the specified method with dependency injection
    /// </summary>
    /// <param name="context">The job execution context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap jobData = context.MergedJobDataMap;

        // string data = jobData.GetString("Data")
        //                          ?? throw new ArgumentException("Service data missing");
        var invocationData = jobData["Data"] as InvocationData ?? throw new ArgumentException("Service data missing");
        Job? job = invocationData.DeserializeJob();
        
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        MethodInfo? method = job.Method;
        
        object? service = method.IsStatic ? null : scope.ServiceProvider.GetRequiredService(job.Type);
        object? result = method.Invoke(service, job.Args.ToArray());

        switch (result)
        {
            case Task task:
                await task.ConfigureAwait(false);
                break;
            // If Task<T> — unwrap
            case ValueTask valueTask:
                await valueTask.ConfigureAwait(false);
                break;
        }
        bool hasNext = jobData.TryGetString("NextJob", out string? nextJob);
        jobData.TryGetInt("NextJobPriority", out int priority);
        if (!hasNext || string.IsNullOrWhiteSpace(nextJob))
        {
            return;
        }
        var trigger = TriggerBuilder.Create()
            .ForJob(nextJob)
            .StartNow()
            .WithPriority(priority)
            .Build();
        await context.Scheduler.ScheduleJob(trigger);
    }
}