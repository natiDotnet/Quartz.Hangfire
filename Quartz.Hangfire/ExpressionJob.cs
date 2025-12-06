using System.Reflection;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Quartz.Hangfire;

public class ExpressionJob(IServiceScopeFactory serviceScopeFactory) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap jobData = context.MergedJobDataMap;

        string serviceTypeName = jobData.GetString("ServiceType")
                                 ?? throw new ArgumentException("Service type missing");

        string methodName = jobData.GetString("Method")
                            ?? throw new ArgumentException("Method name is required.");

        string argsJson = jobData.GetString("Args") ?? "[]";

        Type serviceType = Type.GetType(serviceTypeName)
                           ?? throw new ArgumentException($"Cannot load type {serviceTypeName}");

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        object?[] rawArgs = ArgumentDeserializer.DeserializeArgs(argsJson);

        MethodInfo method = ResolveMethod2(serviceType, methodName, rawArgs)
                            ?? throw new MissingMethodException(serviceType.Name, methodName);
        object? service = method.IsStatic ? null : scope.ServiceProvider.GetRequiredService(serviceType);

        object? result = method.Invoke(service, rawArgs);

        switch (result)
        {
            case Task task:
                await task.ConfigureAwait(false);
                break;
            // If Task<T> — unwrap
            case ValueTask valueTask:
                await valueTask.ConfigureAwait(false);
                break;
        }
    }

    public static MethodInfo? ResolveMethod(Type type, string name, object?[] arguments)
    {
        return type.GetMethods()
            .Where(m => m.Name == name)
            .Where(m =>
            {
                var parameters = m.GetParameters();

                // Length check: allow exact match or +1 for optional CancellationToken at the end
                if (parameters.Length != arguments.Length &&
                    parameters.Length != arguments.Length + 1)
                    return false;

                if (parameters.Length == arguments.Length + 1 && parameters[^1].ParameterType != typeof(CancellationToken))
                    return false;

                // Type compatibility check
                for (int i = 0; i < arguments.Length; i++)
                {
                    var argType = arguments[i]?.GetType() ?? typeof(object);
                    var paramType = parameters[i].ParameterType;

                    if (!paramType.IsAssignableFrom(argType))
                        return false;
                }

                return true;
            })
            .OrderByDescending(m =>
            {
                // 1. Prefer methods that have NO 'object' parameters
                // 2. If both have 'object', prefer the one with fewer of them
                // 3. If tie, prefer shorter parameter list (i.e. no trailing CT)
                int objectCount = m.GetParameters()
                    .Take(arguments.Length) // ignore trailing CT
                    .Count(p => p.ParameterType == typeof(object));

                return -objectCount; // negative = more negative = fewer objects = higher rank
            })
            .ThenBy(m => m.GetParameters().Length) // prefer no CancellationToken
            .FirstOrDefault();
    }
    public static MethodInfo? ResolveMethod2(Type type, string name, object?[] arguments)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

        MethodInfo? best = null;
        int bestObjectCount = int.MaxValue;
        bool bestHasTrailingCt = true; // worse by default
        int argCount = arguments.Length;

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.Name != name) continue;

            var parameters = method.GetParameters();
            int paramCount = parameters.Length;

            // Fast length check
            bool hasTrailingCt = paramCount == argCount + 1 &&
                                 parameters[^1].ParameterType == typeof(CancellationToken);

            if (paramCount != argCount && !hasTrailingCt) continue;

            int matchedParams = hasTrailingCt ? paramCount - 1 : paramCount;

            // Single combined loop: check assignability + count object params
            int objectCount = 0;
            bool compatible = true;

            for (int i = 0; i < argCount; i++)
            {
                var argType = arguments[i]?.GetType() ?? typeof(object);
                var paramType = parameters[i].ParameterType;

                if (!paramType.IsAssignableFrom(argType))
                {
                    compatible = false;
                    break;
                }

                if (paramType == typeof(object))
                    objectCount++;
            }

            if (!compatible) continue;

            // Now decide if this is better than current best
            bool currentHasTrailingCt = hasTrailingCt;
            int currentObjectCount = objectCount;

            bool isBetter =
                currentObjectCount < bestObjectCount || // fewer object params
                (currentObjectCount == bestObjectCount && !currentHasTrailingCt && bestHasTrailingCt) || // same, but no CT
                (currentObjectCount == bestObjectCount && currentHasTrailingCt == bestHasTrailingCt); // tie → stable (or add more rules)

            if (isBetter || best == null)
            {
                best = method;
                bestObjectCount = currentObjectCount;
                bestHasTrailingCt = currentHasTrailingCt;
            }
        }

        return best;
    }
}
