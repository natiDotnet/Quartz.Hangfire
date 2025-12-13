using Microsoft.Extensions.DependencyInjection;
using Quartz.Hangfire.Listeners;
using Quartz.Impl.Matchers;

namespace Quartz.Hangfire.Queue;

public static class QuartzQueueingExtensions
{
    public static void UseQueueing(
        this IServiceCollectionQuartzConfigurator services,
        Action<QuartzQueueOptions> configure)
    {
        var opts = new QuartzQueueOptions();
        configure(opts);
        QuartzQueues.Configure(opts.Queues);
    }

    public static void UseListeners(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IJobExecutionStep, RetryStep>();
        serviceCollection.AddScoped<IJobExecutionStep, NextTriggerStep>();
        quartz.AddJobListener<PipelineJobListener>(
            EverythingMatcher<JobKey>.AllJobs()
        );
    }
}



