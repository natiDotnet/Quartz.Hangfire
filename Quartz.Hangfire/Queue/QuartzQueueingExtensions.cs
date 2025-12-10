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
}

