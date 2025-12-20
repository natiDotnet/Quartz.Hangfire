namespace Quartz.Hangfire.Queue;

using Quartz;

/// <summary>
/// Provides a utility to convert Hangfire cron expressions to the Quartz.NET format.
/// </summary>
public static class CronConverter
{
    /// <summary>
    /// Converts a Hangfire cron expression (5 fields) to a Quartz.NET compatible cron expression (6 fields).
    /// </summary>
    /// <param name="hangfireCron">The Hangfire cron expression (e.g., "* * * * *").</param>
    /// <returns>A Quartz.NET compatible cron expression.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input expression is null, empty, or invalid.
    /// Also thrown if the conversion results in an invalid Quartz expression.
    /// </exception>
    public static string Convert(string hangfireCron)
    {
        if (string.IsNullOrWhiteSpace(hangfireCron))
            throw new ArgumentException("Cron expression is empty.", nameof(hangfireCron));

        var parts = hangfireCron
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Hangfire = exactly 5 fields
        if (parts.Length != 5)
            return hangfireCron;

        var minute = parts[0];
        var hour   = parts[1];
        var dayOfMonth = parts[2];
        var month  = parts[3];
        var dayOfWeek = parts[4];

        // Quartz rule: either DOM or DOW must be '?'
        if (dayOfMonth == "*" && dayOfWeek == "*")
        {
            dayOfWeek = "?";
        }
        else if (dayOfMonth != "*" && dayOfWeek != "*")
        {
            throw new ArgumentException(
                "Quartz cron cannot specify both Day-of-Month and Day-of-Week.");
        }
        else
        {
            dayOfMonth = dayOfMonth == "*" ? "?" : dayOfMonth;
            dayOfWeek  = dayOfWeek == "*"  ? "?" : dayOfWeek;
        }

        // Prepend seconds = 0
        var quartzCron = $"0 {minute} {hour} {dayOfMonth} {month} {dayOfWeek}";

        if (!CronExpression.IsValidExpression(quartzCron))
            throw new ArgumentException($"Invalid Quartz cron generated: '{quartzCron}'");

        return quartzCron;
    }
}
