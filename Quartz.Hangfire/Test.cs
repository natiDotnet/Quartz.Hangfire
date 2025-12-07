namespace Quartz.Hangfire;

public class Test
{
    public async Task<string> TesterAsync(string name, CancellationToken ct)
    {
        await Task.Delay(1000, ct);
        Console.WriteLine($"Hello Tester {name}");
        return $"Hello Tester {name}";
    }
}
