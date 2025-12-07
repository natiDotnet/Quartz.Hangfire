using System.Reflection;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Quartz.Hangfire;

public class ExpressionJob(IServiceScopeFactory serviceScopeFactory) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap jobData = context.MergedJobDataMap;

        string data = jobData.GetString("Data")
                                 ?? throw new ArgumentException("Service data missing");
        var invocationData = InvocationData.DeserializePayload(data);
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
        if (hasNext && !string.IsNullOrWhiteSpace(nextJob))
        {
            await context.Scheduler.TriggerJob(new JobKey(nextJob));
        }

    }
}
