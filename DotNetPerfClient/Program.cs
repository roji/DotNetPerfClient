using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPerfClient
{
    class Program
    {
        const int WarmupTimeSeconds = 3;
        static string Hostname;

        internal static int IsRunning = 1;
        internal static long Counter;

        static void Main(string[] args)
        {
            if (args.Length != 4)
                Usage();
            Hostname = args[0];
            var threadCount = int.Parse(args[1]);
            var seconds = int.Parse(args[2]);

            switch (args[3])
            {
                case "sync":
                    Sync(threadCount, seconds);
                    return;
                case "async":
                    Async(threadCount, seconds);
                    return;
                default:
                    Usage();
                    return;
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine("usage: Client <hostname> <threads> <seconds> <sync or async>");
            Environment.Exit(1);
        }

        static void Sync(int threadCount, int seconds)
        {
            var runners = Enumerable.Range(0, threadCount)
                .Select(i => new SyncRunner(Hostname, 128, 128))
                .ToList();

            foreach (var runner in runners)
                runner.Start();

            Console.WriteLine($"Warming up ({WarmupTimeSeconds} seconds)...");
            Thread.Sleep(WarmupTimeSeconds * 1000);

            Console.WriteLine($"Starting sync benchmark ({threadCount} threads, {seconds} seconds)...");
            Interlocked.Exchange(ref Counter, 0);
            Thread.Sleep(seconds * 1000);
            var transactions = Counter;
            Interlocked.Exchange(ref IsRunning, 0);
            foreach (var thread in runners)
                thread.Join();

            Console.WriteLine($"Transactions per second: {transactions / seconds}");
        }

        static void Async(int threadCount, int seconds)
        {
            var allTasks = Task.WhenAll(
                Enumerable.Range(0, threadCount)
                    .Select(i => new AsyncRunner(Hostname, 128, 128).Run())
            );

            Console.WriteLine($"Warming up ({WarmupTimeSeconds} seconds)...");
            Thread.Sleep(WarmupTimeSeconds * 1000);

            Console.WriteLine($"Starting async benchmark ({threadCount} threads, {seconds} seconds)...");
            Interlocked.Exchange(ref Counter, 0);
            Thread.Sleep(seconds * 1000);
            var transactions = Counter;
            Interlocked.Exchange(ref IsRunning, 0);

            allTasks.GetAwaiter().GetResult();

            Console.WriteLine($"Transactions per second: {transactions / seconds}");
        }
    }
}
