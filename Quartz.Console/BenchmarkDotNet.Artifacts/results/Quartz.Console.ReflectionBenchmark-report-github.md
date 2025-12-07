```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7171/24H2/2024Update/HudsonValley)
12th Gen Intel Core i5-1235U 1.30GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 8.0.411
  [Host]     : .NET 8.0.17 (8.0.17, 8.0.1725.26602), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 8.0.17 (8.0.17, 8.0.1725.26602), X64 RyuJIT x86-64-v3


```
| Method  | Mean     | Error    | StdDev   | Median   | Min      | Max      | Rank | Gen0   | Gen1   | Allocated |
|-------- |---------:|---------:|---------:|---------:|---------:|---------:|-----:|-------:|-------:|----------:|
| JobCall |       NA |       NA |       NA |       NA |       NA |       NA |    ? |     NA |     NA |        NA |
| Enqueue | 35.54 μs | 1.665 μs | 4.777 μs | 33.45 μs | 30.66 μs | 48.57 μs |    1 | 1.3428 | 0.2441 |    8.3 KB |

Benchmarks with issues:
  ReflectionBenchmark.JobCall: DefaultJob
