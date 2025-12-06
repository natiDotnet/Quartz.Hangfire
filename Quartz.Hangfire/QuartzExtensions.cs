using System.Linq.Expressions;
using System.Text.Json;
using Quartz.Console;

namespace Quartz.Hangfire;

public static class QuartzExtensions
{
    public static async Task JobCall(this ISchedulerFactory factory, string name, TimeSpan? delay = null)
    {
        JobDataMap jobData = new()
        {
            {
                "name", name!
            }
        };
        IJobDetail job = JobBuilder.Create<TestJob>()
            .WithIdentity(Guid.NewGuid().ToString())
            .UsingJobData(jobData)
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue)
            triggerBuilder.StartAt(DateTimeOffset.UtcNow.Add(delay.Value));
        else
            triggerBuilder.StartNow();

        ITrigger trigger = triggerBuilder.Build();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.ScheduleJob(job, trigger);
    }
    private static async Task InternalEnqueue(this ISchedulerFactory factory, LambdaExpression expression, TimeSpan? delay = null)
    {
        var body = GetMethodCall(expression);
        Type? serviceType;

        if (body.Method.IsStatic)
        {
            // Static method → no instance needed
            serviceType = body.Method.DeclaringType; // the type that declares the static method
        }
        // GENERIC VERSION:  Expression<Action<T>> or Expression<Func<T, Task>>
        else if (expression.Parameters.Count == 1 &&
            body.Object is ParameterExpression param &&
            param == expression.Parameters[0])
        {
            serviceType = expression.Parameters[0].Type;
        }
        else
        {
            // NON-GENERIC VERSION: Expression<Action> or Expression<Func<Task>>
            // Need to compile just the instance, not the full lambda
            object? target = body.Object != null
                ? Expression.Lambda(body.Object).Compile().DynamicInvoke()
                : null;
            serviceType = target?.GetType()
                              .GetInterfaces()
                              .FirstOrDefault()
                          ?? target?.GetType()
                          ?? null;
        }
        string methodName = body.Method.Name;

        // Evaluate arguments
        var argsWithTypes = body.Arguments
            // .Where(e)
            .Select(expr =>
            {
                // Compile and execute the expression to get the actual value
                var compiled = Expression.Lambda(expr).Compile();
                var value = compiled.DynamicInvoke();

                // Get the actual runtime type (or declared type if value is null)
                var type = value?.GetType() ?? expr.Type;

                return new { Value = value, Type = type.FullName };
            })
            .Where(x => x.Type != typeof(CancellationToken).FullName)  // exclude CancellationToken
            .ToArray();

        JobDataMap jobData = new()
        {
            {
                "ServiceType", serviceType?.AssemblyQualifiedName!
            },
            {
                "Method", methodName
            },
            {
                "Args", JsonSerializer.Serialize(argsWithTypes)
            }
        };

        IJobDetail job = JobBuilder.Create<ExpressionJob>()
            .WithIdentity(Guid.NewGuid().ToString())
            .UsingJobData(jobData)
            .Build();

        var triggerBuilder = TriggerBuilder.Create();
        if (delay.HasValue)
            triggerBuilder.StartAt(DateTimeOffset.UtcNow.Add(delay.Value));
        else
            triggerBuilder.StartNow();

        ITrigger trigger = triggerBuilder.Build();
        IScheduler scheduler = await factory.GetScheduler();
        await scheduler.ScheduleJob(job, trigger);
    }
    
    private static MethodCallExpression GetMethodCall(LambdaExpression expression)
    {
        Expression body = expression.Body;

        // If unary (e.g., Convert)
        if (body is UnaryExpression unary && unary.Operand is MethodCallExpression mc1)
            return mc1;

        // If direct method call
        if (body is MethodCallExpression mc2)
            return mc2;

        // If block with single expression
        if (body is BlockExpression block && block.Expressions.Count == 1 && block.Expressions[0] is MethodCallExpression mc3)
            return mc3;

        throw new ArgumentException("Expression must be a method call");
    }

    
    public static async Task Schedule<TService>(this ISchedulerFactory factory, Expression<Func<TService, Task>> expression, TimeSpan delay)
        where TService : notnull
    {
        await InternalEnqueue(factory, expression, delay);
    }
    
    public static async Task Schedule<TService>(this ISchedulerFactory factory, Expression<Action<TService>> expression, TimeSpan delay)
        where TService : notnull
    {
        await InternalEnqueue(factory, expression, delay);
    }

    public static async Task Enqueue(
        this ISchedulerFactory factory,
        Expression<Func<Task>> expression)
    {
        await factory.InternalEnqueue(expression);
    }

    public static async Task Enqueue(
        this ISchedulerFactory factory,
        Expression<Action> expression)
    {
        await factory.InternalEnqueue(expression);
    }

    // Generic strongly-typed lambda
    public static Task Enqueue<T>(
        this ISchedulerFactory factory,
        Expression<Action<T>> expression) where T : notnull
    {
        return InternalEnqueue(factory, expression);
    }

    public static Task Enqueue<T>(
        this ISchedulerFactory factory,
        Expression<Func<T, Task>> expression) where T : notnull
    {
        return InternalEnqueue(factory, expression);
    }
}
