// See https://aka.ms/new-console-template for more information

// using BenchmarkDotNet.Running;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Hangfire;
using Quartz.Console;
using Quartz.Hangfire.Queue;
using Quartz.Impl;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Test>();
        // services.AddTransient<ReflectionBenchmark>();
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UsePersistentStore(p =>
            {
                p.UseNewtonsoftJsonSerializer();
                p.UsePostgres("Host=localhost;Port=5432;Database=quartz;Username=postgres;Password=root");
            });
            q.UseListeners(services);
            q.UseQueueing(c => c.Queues = ["critical", "high", "default", "low"]);
        });
        services.AddQuartzHostedService();
        services.AddHangfire(config =>
        {
            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage();
        });
        
        // Background server
        services.AddHangfireServer();

    })
    .Build();

await host.StartAsync();

var scheduler = host.Services.GetRequiredService<ISchedulerFactory>();
// await scheduler.Enqueue<Test>(t => t.TesterAsync("nano", CancellationToken.None));// here it is!

await Task.Delay(5000);
// await scheduler.Enqueue<Test>(t => t.TesterAsync("big", CancellationToken.None));// here it is!
// await scheduler.Enqueue("first", () => Console.WriteLine($"Hello World {DateTime.Now}"));
// var triggerKey = await scheduler.Schedule("critical", () => Console.WriteLine($"Hello After {DateTime.Now}"), TimeSpan.FromMinutes(1));
// await scheduler.ContinueJobWith(triggerKey, "critical", () => Console.WriteLine($"Hello After Hello {DateTime.Now}"), JobContinuationOptions.OnlyOnDeletedState);
// await QuartzJob.Enqueue(() => Console.WriteLine($"Hello World {DateTime.Now}"));
// // Now you can enqueue anything
// // BenchmarkRunner.Run<ReflectionBenchmark>();
// BackgroundJob.ContinueJobWith("test", () => Console.WriteLine("test"));
await scheduler.AddOrUpdate<Test>("tt", t => t.TesterAsync("rec"), Cron.Daily);
RecurringJob.TriggerJob("test");
await host.RunAsync();

// await Parallel.ForEachAsync(Enumerable.Range(1, 1000), async (i, c) =>
// {
//     await DotNetCli.RestoreAsync(
//         workingDirectory: @"D:\ArifPay\Quartz.Hangfire\Quartz.Console",
//         cancellationToken: c);
//     Console.WriteLine(i);
// });
