namespace Quartz.Console;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class DotNetCli
{
    public static async Task RestoreAsync(
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "restore",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var outputTask = ReadStreamAsync(process.StandardOutput, cancellationToken);
        var errorTask = ReadStreamAsync(process.StandardError, cancellationToken);

        await Task.WhenAll(
            process.WaitForExitAsync(cancellationToken),
            outputTask,
            errorTask
        );

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet restore failed with exit code {process.ExitCode}");
        }
    }

    private static async Task ReadStreamAsync(
        System.IO.StreamReader reader,
        CancellationToken cancellationToken)
    {
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is not null)
            {
                Console.WriteLine(line);
            }
        }
    }
}

