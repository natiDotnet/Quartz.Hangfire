using BenchmarkDotNet.Attributes;
using Hangfire;
using Hangfire.MemoryStorage;
using Quartz.Hangfire;
using Quartz.Impl;

namespace Quartz.Console;
[MemoryDiagnoser]
[RankColumn, MinColumn, MaxColumn]
public class ReflectionBenchmark
{
    private readonly ISchedulerFactory _factory;
    private readonly IBackgroundJobClient _hangfire;
    public ReflectionBenchmark()
    {
        JobStorage.Current = new MemoryStorage();
        _factory = new StdSchedulerFactory();
        _hangfire = new BackgroundJobClient();
    }
    
    // [Benchmark(Baseline = true)]
    // public Task QuartzJobCall() => _factory.JobCall("nati");
    [Benchmark]
    public Task QuartzEnqueue() => _factory.Enqueue<Test>(t => t.TesterAsync("nati", CancellationToken.None));
    // [Benchmark]
    // public string HangfireEnqueue() => _hangfire.Enqueue<Test>(t => t.Tester("nati"));
}
