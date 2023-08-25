using System.Diagnostics;
using GIT.Utilities;
namespace Sample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var mainTask = Task.Delay(TimeSpan.FromSeconds(5));
            Stopwatch sw = Stopwatch.StartNew();
            Action tickDelegate = () =>
            {
                Console.WriteLine($"Time since main task started: {sw.Elapsed.TotalSeconds} seconds");
            };

            await mainTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickDelegate);
            sw.Stop();
            Console.WriteLine($"Main task finished in {sw.Elapsed.TotalSeconds} seconds");

            var resultTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                return "Finished";
            });
            var result = await resultTask.RunWithTimedCallback(TimeSpan.FromSeconds(1), tickDelegate);
            Console.WriteLine($"Result task finished in {sw.Elapsed.TotalSeconds} seconds and returned \"{result}\"");

        }
    }
}