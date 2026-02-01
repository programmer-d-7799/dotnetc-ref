using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Async demo starting...");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        try
        {
            await RunConcurrentWorkAsync(cts.Token);
            await RunConcurrentWorkWhenAnyAsync(cts.Token);
            await StreamSquaresAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Canceled by token.");
        }

        Console.WriteLine("Async demo finished.");
    }

    private static async Task RunConcurrentWorkAsync(CancellationToken token)
    {
        Console.WriteLine("\nTask.WhenAll demo:");

        var sw = Stopwatch.StartNew();

        var tasks = new[]
        {
            SimulatedIoAsync("A", 400, token),
            SimulatedIoAsync("B", 650, token),
            SimulatedIoAsync("C", 250, token)
        };

        var results = await Task.WhenAll(tasks);

        sw.Stop();
        Console.WriteLine($"Results: {string.Join(", ", results)}");
        Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
    }

    private static async Task RunConcurrentWorkWhenAnyAsync(CancellationToken token)
    {
        Console.WriteLine("\nTask.WhenAny demo:");

        var sw = Stopwatch.StartNew();

        var pending = new List<Task<string>>
        {
            SimulatedIoAsync("A", 400, token),
            SimulatedIoAsync("B", 650, token),
            SimulatedIoAsync("C", 250, token)
        };

        var results = new List<string>(pending.Count);

        while (pending.Count > 0)
        {
            var completed = await Task.WhenAny(pending);
            pending.Remove(completed);

            var result = await completed;
            results.Add(result);
            Console.WriteLine($"Completed: {result}");
        }

        sw.Stop();
        Console.WriteLine($"Results: {string.Join(", ", results)}");
        Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
    }

    private static async Task<string> SimulatedIoAsync(string name, int delayMs, CancellationToken token)
    {
        await Task.Delay(delayMs, token);
        return $"{name}:{delayMs}ms";
    }

    private static async Task StreamSquaresAsync(CancellationToken token)
    {
        Console.WriteLine("\nawait foreach demo:");

        await foreach (var value in GenerateSquaresAsync(token))
        {
            Console.WriteLine($"Square: {value}");
        }
    }

    private static async IAsyncEnumerable<int> GenerateSquaresAsync(
        [EnumeratorCancellation] CancellationToken token)
    {
        for (int i = 1; i <= 5; i++)
        {
            await Task.Delay(200, token);
            yield return i * i;
        }
    }
}
