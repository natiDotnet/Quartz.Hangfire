using System.Reflection;
using Hangfire;
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
        var invocationData = context.MergedJobDataMap["Data"] as InvocationData ?? throw new ArgumentException("Service data missing");
        var job = invocationData.DeserializeJob();

        var executionException = await ExecuteJobInternalAsync(job);

        await HandleContinuationAsync(context, executionException);
    }

    private async Task<JobExecutionException?> ExecuteJobInternalAsync(Job job)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var method = job.Method;
            var service = method.IsStatic ? null : scope.ServiceProvider.GetRequiredService(job.Type);
            var result = method.Invoke(service, job.Args.ToArray());

            switch (result)
            {
                case Task task:
                    await task.ConfigureAwait(false);
                    break;
                case ValueTask valueTask:
                    await valueTask.ConfigureAwait(false);
                    break;
            }

            return null;
        }
        catch (Exception ex)
        {
            return new JobExecutionException(ex);
        }
    }
    
    private static async Task HandleContinuationAsync(IJobExecutionContext context, JobExecutionException? exception)
    {
        var jobData = context.MergedJobDataMap;

        if (!jobData.TryGetString("NextJob", out var nextKey) || string.IsNullOrWhiteSpace(nextKey))
        {
            if (exception != null) throw exception;
            return;
        }

        var options = JobContinuationOptions.OnlyOnSucceededState;
        if (jobData.TryGetInt("Options", out var value))
        {
            options = (JobContinuationOptions)value;
        }
        var state = exception is null ? JobContinuationOptions.OnlyOnSucceededState : JobContinuationOptions.OnlyOnDeletedState;

        if (options != JobContinuationOptions.OnAnyFinishedState && options != state)
        {
            if (exception != null) throw exception;
            return;
        }
        jobData.TryGetInt("NextJobPriority", out int priority);
        var trigger = TriggerBuilder.Create()
            .ForJob(nextKey)
            .StartNow()
            .WithPriority(priority)
            .Build();
        await context.Scheduler.ScheduleJob(trigger);

        if (exception != null)
        {
            throw exception;
        }
    }
}