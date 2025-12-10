using System.Diagnostics.CodeAnalysis;

namespace Quartz.Hangfire.Queue;

public sealed class QuartzQueueOptions
{
    public string[] Queues { get; set; } = ["default"];
}

internal static class QuartzQueues
{
    [SuppressMessage("ReSharper", "InconsistentNaming")] 
    private static readonly Dictionary<string, int> _queues = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, int> Queues => _queues;

    /// <summary>
    /// Configure queues in priority order.
    /// Example: Configure("critical", "high", "default", "low")
    /// Automatically assigns values 10..1, default is always 5
    /// </summary>
    internal static void Configure(params string[] queues)
    {
        _queues.Clear();

        int n = queues.Length;
        for (int i = 0; i < n; i++)
        {
            var name = queues[i].Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            int value;
            value = name.Equals("default", StringComparison.OrdinalIgnoreCase) ? 5 : // default priority
                // scale linearly from 10 down to 1
                Math.Clamp(10 - i * 8 / Math.Max(1, n - 1), 1, 10);

            _queues[name] = value;
        }

        // Ensure "default" always exists
        _queues.TryAdd("default", 5);
    }
    
    /// <summary>
    /// Get priority of a queue, fallback to default
    /// </summary>
    public static int GetPriority(string? queue)
    {
        if (queue != null && _queues.TryGetValue(queue, out var value))
            return value;

        return _queues.GetValueOrDefault("default", 5);
    }
}

