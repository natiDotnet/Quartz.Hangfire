namespace Quartz.Hangfire.Tests.Helpers;

public static class TestJob
{
    public static void Execute()
    {
        // A dummy method for testing purposes
    }

    public static async Task ExecuteAsync()
    {
        // A dummy async method for testing purposes
        await Task.CompletedTask;
    }
}
