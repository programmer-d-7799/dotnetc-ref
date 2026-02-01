using System;
using System.Collections.Concurrent;
using System.Threading;

internal static class Program
{
    private const int WorkerCount = 4;
    private const int Iterations = 200_000;

    private sealed record ThreadFailure(string ThreadName, Exception Exception);

    private static void Main()
    {
        Console.WriteLine("Thread control demo starting...");

        CommunicationDemo();
        SharedMemoryDemo();
        ExceptionHandlingDemo();

        Console.WriteLine("Thread control demo finished.");
    }

    private static void CommunicationDemo()
    {
        Console.WriteLine("\nThread communication demo:");

        using var mailbox = new BlockingCollection<string>(new ConcurrentQueue<string>());
        using var startGate = new ManualResetEventSlim(false);

        var producer = new Thread(() =>
        {
            startGate.Wait();
            for (int i = 1; i <= 5; i++)
            {
                string message = $"message-{i}";
                mailbox.Add(message);
                Console.WriteLine($"Producer sent {message} on thread {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(80);
            }
            mailbox.CompleteAdding();
        })
        {
            Name = "producer"
        };

        var consumer = new Thread(() =>
        {
            startGate.Wait();
            foreach (var message in mailbox.GetConsumingEnumerable())
            {
                Console.WriteLine($"Consumer received {message} on thread {Thread.CurrentThread.ManagedThreadId}");
            }
        })
        {
            Name = "consumer"
        };

        producer.Start();
        consumer.Start();
        startGate.Set();

        producer.Join();
        consumer.Join();
    }

    private static void SharedMemoryDemo()
    {
        Console.WriteLine("\nShared memory demo:");

        int unsafeCounter = 0;
        int safeCounter = 0;

        var threads = new Thread[WorkerCount];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < Iterations; j++)
                {
                    unsafeCounter++;
                    Interlocked.Increment(ref safeCounter);
                }
            })
            {
                Name = $"counter-{i + 1}"
            };
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        int expected = WorkerCount * Iterations;

        Console.WriteLine($"Expected increments: {expected}");
        Console.WriteLine($"Unsafe counter:      {unsafeCounter}");
        Console.WriteLine($"Safe counter:        {safeCounter}");
        Console.WriteLine("Unsafe counter may match expected by chance, but it is not thread-safe.");
    }

    private static void ExceptionHandlingDemo()
    {
        Console.WriteLine("\nException handling demo:");

        using var cts = new CancellationTokenSource();
        var failures = new ConcurrentQueue<ThreadFailure>();

        var listener = StartThread(
            "listener",
            () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    Thread.Sleep(40);
                }
                Console.WriteLine("Listener observed cancellation.");
            },
            failures);

        var worker = StartThread(
            "faulty-worker",
            () =>
            {
                Thread.Sleep(120);
                throw new InvalidOperationException("Simulated failure.");
            },
            failures,
            cts);

        listener.Join();
        worker.Join();

        if (failures.IsEmpty)
        {
            Console.WriteLine("No exceptions detected.");
            return;
        }

        Console.WriteLine("Exceptions detected from threads:");
        while (failures.TryDequeue(out var failure))
        {
            Console.WriteLine($"- {failure.ThreadName}: {failure.Exception.GetType().Name} - {failure.Exception.Message}");
        }
    }

    private static Thread StartThread(
        string name,
        Action action,
        ConcurrentQueue<ThreadFailure> failures,
        CancellationTokenSource? cancelOnFailure = null)
    {
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failures.Enqueue(new ThreadFailure(name, ex));
                cancelOnFailure?.Cancel();
            }
        })
        {
            Name = name,
            IsBackground = false
        };

        thread.Start();
        return thread;
    }
}
