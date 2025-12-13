using Hangfire;
using Microsoft.Extensions.Logging;

namespace Quartz.Hangfire.Listeners;

public sealed class RetryStep(ILogger<RetryStep> logger) : IJobExecutionStep
{
    internal const string RetryKey = "RetryCount";
    internal const string MaxRetries = "MaxRetries";

    public Task OnExecuting(
        IJobExecutionContext context,
        CancellationToken cancellationToken,
        Func<Task> next)
        => Task.CompletedTask;

    public Task OnExecuted(
        IJobExecutionContext context,
        JobExecutionException? exception,
        CancellationToken cancellationToken,
        Func<Task> next)
    {
        if (exception is null)
        {
            return next();
        }

        var trigger = context.Trigger;
        trigger.JobDataMap.TryGetInt(RetryKey, out int retry);
        var retryAttribute = trigger.JobDataMap["Retry"] as AutomaticRetryAttribute;

        if (retry >= retryAttribute!.Attempts)
        {
            return next();
        }
        logger.LogInformation("Retrying job {Key} try {Retry}", trigger.Key, retry);

        int delaySeconds = ComputeDelay(retry, retryAttribute.DelaysInSeconds);

        var newTrigger = trigger
            .GetTriggerBuilder()
            .UsingJobData(RetryKey, retry + 1)
            .StartAt(DateBuilder.FutureDate(delaySeconds, IntervalUnit.Second))
            .Build();

        return context.Scheduler.RescheduleJob(trigger.Key, newTrigger, cancellationToken);
    }
    private static int ComputeDelay(int retry, int[]? delays)
        => delays switch
        {
            { Length: > 0 } when retry < delays.Length => delays[retry],
            { Length: > 0 } => delays[^1],           // retry >= Length
            _ => (int)Math.Pow(2, retry)  // null or empty array
        };
}

