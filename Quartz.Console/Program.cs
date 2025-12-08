// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Hangfire;
using Quartz.Impl;
using Quartz;
using Quartz.Console;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Test>();
        services.AddTransient<ReflectionBenchmark>();
        services.AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); });
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
// BackgroundJob.Enqueue<Test>(e => e.Tester("nati"));
// await scheduler.Enqueue<Test>(t => t.TesterAsync("nano", CancellationToken.None));// here it is!
// await scheduler.Enqueue(() => Console.WriteLine($"Hello World {DateTime.Now}"));
// await scheduler.Schedule("hello", () => Console.WriteLine($"Hello After {DateTime.Now}"), TimeSpan.FromMinutes(1));
// await scheduler.ContinueJobWith(new JobKey("hello"), () => Console.WriteLine($"Hello After Hello {DateTime.Now}"));
// Now you can enqueue anything
// JobStorage.Current = new MemoryStorage();
BenchmarkRunner.Run<ReflectionBenchmark>();
// await scheduler.Enqueue<Test>(t => t.Tester("Nati"));
await host.RunAsync();
