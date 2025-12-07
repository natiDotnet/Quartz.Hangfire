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
    // private static readonly Type TestType = typeof(Test);
    // private static readonly string MethodName = nameof(Test.Tester);
    // private static readonly object?[] SampleArgs = { "nati" };  // Triggers 2-param overload

    private readonly ISchedulerFactory _factory = new StdSchedulerFactory();
    [Benchmark(Baseline = true)]
    public Task JobCall() => _factory.JobCall("nati");
    [Benchmark]
    public Task Enqueue() => _factory.Enqueue<Test>(t => t.Tester("nati"));
    [Benchmark]
    public string Hangfire()
    {
        JobStorage.Current = new MemoryStorage();
        return BackgroundJob.Enqueue<Test>(t => t.Tester("nati"));
    }
    //     [Benchmark]
//     public MethodInfo? ResolveMethod() => ExpressionJob.ResolveMethod(TestType, MethodName, SampleArgs);
//     [Benchmark]
//     public MethodInfo? ResolveMethod2() => ExpressionJob.ResolveMethod2(TestType, MethodName, SampleArgs);
}
