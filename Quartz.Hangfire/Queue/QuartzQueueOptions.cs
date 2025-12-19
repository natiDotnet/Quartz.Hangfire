using System.Diagnostics.CodeAnalysis;

namespace Quartz.Hangfire.Queue;

/// <summary>
/// Represents configuration options for defining and managing Quartz job queues.
/// </summary>
public sealed class QuartzQueueOptions
{
    /// <summary>
    /// Gets or sets the list of queue names to be processed.
    /// </summary>
    /// <remarks>
    /// The default value is <c>["default"]</c>. 
    /// When configuring multiple queues, the order in this array may influence processing priority depending on the consumer implementation.
    /// </remarks>
    public string[] Queues { get; set; } = ["default"];
}

/// <summary>
/// Internal helper class for managing queue definitions and their associated priorities.
/// </summary>
internal static class QuartzQueues
{
    [SuppressMessage("ReSharper", "InconsistentNaming")] 
    private static readonly Dictionary<string, int> _queues = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a read-only dictionary of configured queues and their assigned priority values.
    /// </summary>
    public static IReadOnlyDictionary<string, int> Queues
    {
        get => _queues;
    }

    /// <summary>
    /// Configures the available queues and assigns priority values based on the provided order.
    /// </summary>
    /// <param name="queues">An array of queue names, ordered from the highest priority to the lowest priority.</param>
    /// <remarks>
    /// <para>
    /// The method clears existing configurations and recalculates priorities.
    /// The "default" queue is explicitly assigned a priority of 5.
    /// Other queues are assigned values scaled linearly from 10 down to 1 based on their index in the array.
    /// </para>
    /// <para>
    /// Example: <c>Configure("critical", "high", "default", "low")</c>
    /// </para>
    /// </remarks>
    internal static void Configure(params string[] queues)
    {
        _queues.Clear();

        int n = queues.Length;
        for (int i = 0; i < n; i++)
        {
            var name = queues[i].Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            int value = name.Equals("default", StringComparison.OrdinalIgnoreCase) ? 5 : // default priority
                // scale linearly from 10 down to 1
                Math.Clamp(10 - i * 8 / Math.Max(1, n - 1), 1, 10);

            _queues[name] = value;
        }

        // Ensure "default" always exists
        _queues.TryAdd("default", 5);
    }
    
    /// <summary>
    /// Retrieves the priority value associated with a specific queue name.
    /// </summary>
    /// <param name="queue">The name of the queue to retrieve the priority for.</param>
    /// <returns>
    /// The integer priority value of the queue. 
    /// If the queue is <c>null</c> or not found, returns the priority of the "default" queue (typically 5).
    /// </returns>
    public static int GetPriority(string? queue)
    {
        if (queue != null && _queues.TryGetValue(queue, out var value))
            return value;

        return _queues.GetValueOrDefault("default", 5);
    }
}
