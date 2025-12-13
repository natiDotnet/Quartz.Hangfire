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
    public Task Execute(IJobExecutionContext context)
    {
        var invocationData = context.MergedJobDataMap["Data"] as InvocationData ?? throw new ArgumentException("Service data missing");
        var job = invocationData.DeserializeJob();

        return ExecuteJobInternalAsync(job);
    }

    private async Task ExecuteJobInternalAsync(Job job)
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
    }
}
