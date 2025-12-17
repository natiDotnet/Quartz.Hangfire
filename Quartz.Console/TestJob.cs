using Microsoft.Extensions.DependencyInjection;

namespace Quartz.Console;

[DisallowConcurrentExecution]
public class TestJob(IServiceScopeFactory scopeFactory) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap jobData = context.MergedJobDataMap;

        string name = jobData.GetString("name")
                                 ?? throw new ArgumentException("name is required");
        using IServiceScope scope = scopeFactory.CreateScope();
        Test tester = scope.ServiceProvider.GetRequiredService<Test>();

        await tester.TesterAsync(name, context.CancellationToken);
    }
}
