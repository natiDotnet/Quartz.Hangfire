# QuartzHangfire.Extensions

[![NuGet](https://img.shields.io/nuget/v/QuartzHangfire.Extensions.svg)](https://www.nuget.org/packages/QuartzHangfire.Extensions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

QuartzHangfire.Extensions is a .NET library that brings Hangfire-like syntax and functionality to Quartz.NET, allowing developers to easily schedule and manage background jobs using familiar method signatures.

## Features

- **Hangfire-like Syntax**: Use familiar Hangfire-style method calls with Quartz.NET under the hood
- **Immediate Execution**: Enqueue jobs for immediate execution
- **Delayed Execution**: Schedule jobs to run after a specified delay
- **Scheduled Execution**: Schedule jobs to run at a specific time
- **Job Continuation**: Chain jobs together to run one after another
- **Queue Support**: Assign jobs to specific queues
- **Generic Method Support**: Works with both static and instance methods
- **Async/Await Support**: Full support for asynchronous methods

## Installation

Install the package from NuGet:

```bash
dotnet add package QuartzHangfire.Extensions
```

Or via Package Manager Console:

```powershell
Install-Package QuartzHangfire.Extensions
```

## Setup

To use Quartz.Extensions.Hangfire, configure Quartz in your application's service collection:

```csharp
using Quartz;
using Quartz.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(q => 
{ 
    q.UseMicrosoftDependencyInjectionJobFactory(); 
});

builder.Services.AddQuartzHostedService();

// Get the scheduler factory from the service provider
var app = builder.Build();
ISchedulerFactory schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
```

## Usage Examples

### Enqueue Immediate Jobs

```csharp
// Enqueue a static method
await schedulerFactory.Enqueue(() => Console.WriteLine("Hello World"));

// Enqueue an instance method
await schedulerFactory.Enqueue<MyService>(x => x.ProcessDataAsync());

// Enqueue with a specific queue
await schedulerFactory.Enqueue("critical", () => CriticalOperation());
```

### Schedule Delayed Jobs

```csharp
// Schedule a job to run after 5 minutes
await schedulerFactory.Schedule(() => SendEmail(), TimeSpan.FromMinutes(5));

// Schedule with a specific queue
await schedulerFactory.Schedule("email", () => SendEmail(), TimeSpan.FromMinutes(5));
```

### Schedule Jobs at Specific Times

```csharp
// Schedule a job to run at a specific time
await schedulerFactory.Schedule(() => GenerateReport(), new DateTime(2025, 12, 31, 23, 59, 59));

// Schedule with a specific queue
await schedulerFactory.Schedule("reports", () => GenerateReport(), new DateTime(2025, 12, 31, 23, 59, 59));
```

### Continue Jobs with Chained Execution

```csharp
// Schedule the first job with a specific name so we can reference it later
await schedulerFactory.Schedule("first-job", () => Console.WriteLine("First job"), TimeSpan.FromMinutes(1));

// Continue with a second job by referencing the first job by its name
await schedulerFactory.ContinueJobWith(new JobKey("first-job"), () => Console.WriteLine("Second job"));
```

### Working with Generic Types

```csharp
// Enqueue instance methods with parameters
await schedulerFactory.Enqueue<MyService>(x => x.ProcessOrder(orderId));

// Schedule async methods
await schedulerFactory.Schedule<MyService>(x => x.ProcessOrderAsync(orderId), TimeSpan.FromMinutes(10));
```

## Supported Method Signatures

The library supports various method signatures:

- `Action` - Synchronous methods without parameters
- `Func<Task>` - Asynchronous methods without parameters
- `Action<T>` - Synchronous methods with parameters
- `Func<T, Task>` - Asynchronous methods with parameters

## Requirements

- .NET 8.0, 9.0, or 10.0
- Quartz.NET 3.15.1 or higher
- Hangfire.Core 1.8.22 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Quartz.NET](https://www.quartz-scheduler.net/) - The powerful job scheduling system for .NET
- [Hangfire](https://www.hangfire.io/) - Easy background job processing for .NET