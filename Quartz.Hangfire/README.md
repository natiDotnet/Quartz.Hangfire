# QuartzHangfire.Extensions

[![NuGet](https://img.shields.io/nuget/v/QuartzHangfire.Extensions.svg)](https://www.nuget.org/packages/QuartzHangfire.Extensions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

QuartzHangfire.Extensions is a .NET library that brings Hangfire-like syntax and functionality to Quartz.NET, allowing developers to easily schedule and manage background jobs using familiar method signatures.

## Features

- **Hangfire-like Syntax**: Use familiar Hangfire-style method calls with Quartz.NET under the hood.
- **Immediate Execution**: Enqueue jobs for immediate execution.
- **Delayed Execution**: Schedule jobs to run after a specified delay.
- **Job Continuation**: Chain jobs together to run one after another.
- **Job Deletion**: Delete jobs by their `JobKey`.
- **Job Rescheduling**: Reschedule jobs with a new trigger time.
- **Queue Support**: Assign jobs to specific queues and configure queue priorities.
- **Async/Await Support**: Full support for asynchronous methods.

## Installation

Install the package from NuGet:

```bash
dotnet add package QuartzHangfire.Extensions
```

Or via the Package Manager Console:

```powershell
Install-Package QuartzHangfire.Extensions
```

## Performance

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7171/24H2/2024Update/HudsonValley)
12th Gen Intel Core i5-1235U 1.30GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 10.0.100

```
| Method         | Mean     | Error    | StdDev   | Median   | Min       | Max      | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |---------:|---------:|---------:|---------:|----------:|---------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| QuartzJobCall  | 11.58 us | 0.749 us | 2.148 us | 10.85 us |  9.083 us | 19.27 us |  1.03 |    0.26 |    1 | 0.5188 | 0.1373 |   3.18 KB |        1.00 |
| QuartzHangfire | 13.56 us | 0.271 us | 0.509 us | 13.50 us | 12.543 us | 14.85 us |  1.21 |    0.20 |    2 | 1.3428 | 0.3662 |    8.3 KB |        2.61 |
| HangfireDi     | 12.17 us | 0.238 us | 0.284 us | 12.07 us | 11.810 us | 12.77 us |  1.08 |    0.18 |    1 | 2.5635 | 0.6714 |  15.83 KB |        4.98 |
| HangfireStatic | 27.63 us | 0.318 us | 0.265 us | 27.55 us | 27.161 us | 28.08 us |  2.46 |    0.40 |    3 | 3.0518 | 0.7324 |  19.44 KB |        6.11 |

```

## Setup

To use QuartzHangfire.Extensions, configure Quartz in your application's service collection.

### Basic Configuration

At a minimum, you need to register Quartz and the hosted service.

```csharp
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(q => 
{ 
    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();
ISchedulerFactory schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
```

### Optional: Configuring Queues

For job prioritization, you can optionally configure queues using the `UseQueueing` extension. The order of queues in the array determines their priority (from highest to lowest).

```csharp
using Quartz.Hangfire; // Make sure to include this namespace

builder.Services.AddQuartz(q => 
{ 
    q.UseMicrosoftDependencyInjectionJobFactory();
    
    // This is optional. If not configured, all jobs run with default priority.
    q.UseQueueing(c => c.Queues = ["critical", "high", "default", "low"]);
});
```

- Jobs enqueued to `"critical"` will be processed before jobs in `"high"`, and so on.
- If a job is enqueued without a specific queue, it will be placed in the `"default"` queue.
- If `UseQueueing` is not called, all jobs will run with the default Quartz priority.

## Usage Examples

### Enqueue Immediate Jobs

Jobs that should run as soon as possible.

```csharp
// Enqueue a static method to the "default" queue
var triggerKey = await schedulerFactory.Enqueue(() => Console.WriteLine("Hello World"));

// Enqueue an instance method of a registered service
await schedulerFactory.Enqueue<MyService>(x => x.ProcessDataAsync());

// Enqueue to a specific queue
await schedulerFactory.Enqueue("critical", () => CriticalOperation());
```

### Schedule Delayed Jobs

Jobs that should run after a specified delay.

```csharp
// Schedule a job to run after 5 minutes in the "default" queue
var triggerKey = await schedulerFactory.Schedule(() => SendEmail(), TimeSpan.FromMinutes(5));

// Schedule with a specific queue
await schedulerFactory.Schedule("emails", () => SendEmail(), TimeSpan.FromMinutes(5));
```

### Continue Jobs with Chained Execution

Create a continuation job that runs after a parent job completes.

```csharp
// 1. Schedule the first job
var parentTriggerKey = await schedulerFactory.Schedule(() => Console.WriteLine("First job"), TimeSpan.FromMinutes(1));

// 2. Chain a second job to run after the first one completes
await schedulerFactory.ContinueJobWith(parentTriggerKey, () => Console.WriteLine("Second job"));

// You can also use a string representation of the trigger key
await schedulerFactory.ContinueJobWith(parentTriggerKey.ToString(), () => Console.WriteLine("Another continuation"));
```

### Delete a Job

Remove a job from the scheduler using its `JobKey`.

```csharp
// First, get the JobKey from the trigger
var trigger = await scheduler.GetTrigger(triggerKey);
if (trigger != null)
{
    var jobKey = trigger.JobKey;
    
    // Delete the job
    bool deleted = await schedulerFactory.Delete(jobKey);
    if (deleted)
    {
        Console.WriteLine("Job deleted successfully.");
    }
}
```

### Reschedule a Job

Change the schedule of an existing job.

```csharp
// Reschedule a job to run at a new time (e.g., 10 minutes from now)
bool rescheduled = await schedulerFactory.Reschedule(triggerKey, TimeSpan.FromMinutes(10));
if (rescheduled)
{
    Console.WriteLine("Job rescheduled successfully.");
}
```

## Supported Method Signatures

The library supports various method signatures for both static and instance methods:

- `Expression<Action>`
- `Expression<Func<Task>>`
- `Expression<Action<T>>`
- `Expression<Func<T, Task>>`

## Requirements

- .NET 8.0 or .NET 9.0
- Quartz.NET 3.8.1 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/AmazingFeature`).
3. Commit your changes (`git commit -m 'Add some amazing feature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Quartz.NET](https://www.quartz-scheduler.net/) - The powerful job scheduling system for .NET.
- [Hangfire](https://www.hangfire.io/) - The library that inspired this project's syntax.
```