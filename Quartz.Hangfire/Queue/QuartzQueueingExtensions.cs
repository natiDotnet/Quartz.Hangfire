using Microsoft.Extensions.DependencyInjection;
using Quartz.Hangfire.Listeners;
using Quartz.Impl.Matchers;

namespace Quartz.Hangfire.Queue;

/// <summary>
/// Provides extension methods for configuring Quartz.NET with Hangfire-like queueing and listener capabilities.
/// </summary>
public static class QuartzQueueingExtensions
{
    /// <summary>
    /// Configures queue options for Quartz jobs, enabling queue-based job processing.
    /// </summary>
    /// <param name="services">The Quartz configurator to attach the queueing configuration to.</param>
    /// <param name="configure">A delegate to configure the <see cref="QuartzQueueOptions"/>.</param>
    public static void UseQueueing(
        this IServiceCollectionQuartzConfigurator services,
        Action<QuartzQueueOptions> configure)
    {
        var opts = new QuartzQueueOptions();
        configure(opts);
        QuartzQueues.Configure(opts.Queues);
    }

    /// <summary>
    /// Registers default job execution steps and listeners for the Quartz scheduler.
    /// </summary>
    /// <remarks>
    /// This includes steps for disabling concurrent execution, automatic retries, and job continuation,
    /// as well as registering the pipeline job listener and concurrency trigger listener.
    /// </remarks>
    /// <param name="quartz">The Quartz configurator.</param>
    /// <param name="serviceCollection">The application service collection to register scoped execution steps.</param>
    public static void UseListeners(this IServiceCollectionQuartzConfigurator quartz, IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IJobExecutionStep, DisableConcurrentExecution>();
        serviceCollection.AddScoped<IJobExecutionStep, AutomaticRetry>();
        serviceCollection.AddScoped<IJobExecutionStep, ContinueJob>();
        quartz.AddJobListener<PipelineJobListener>(
            EverythingMatcher<JobKey>.AllJobs()
        );
        
        quartz.AddTriggerListener<ConcurrencyTriggerListener>(
            EverythingMatcher<TriggerKey>.AllTriggers()
        );
    }
}
