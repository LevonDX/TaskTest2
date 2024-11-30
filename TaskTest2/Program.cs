
using System.Runtime.CompilerServices;

namespace TaskTest2
{
    internal class Program
    {
        static void DownloadFile(int fileSize, CancellationToken token)
        {
            Console.WriteLine("Download started");
            int i = 0;

            while (i < 5)
            {
                i++;
                Console.WriteLine("Downloading");
                Thread.Sleep(fileSize);

                if (token.IsCancellationRequested)
                {
                    // return;
                    token.ThrowIfCancellationRequested(); // throws an exception if the token is cancelled
                }
            }
        }

        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            try
            {
                // Create a task, pass the DownloadFile method and token to be able to cancel the task
                Task task = new Task(() => DownloadFile(1000, token), token);

                // explicitly start the task
                task.Start();

                Console.WriteLine("Main thread is running");

                TaskAwaiter awaiter = task.GetAwaiter(); // get the awaiter from the task

                // register a continuation to be executed when the task finishes, using the awaiter
                awaiter.OnCompleted(() =>
                {
                    if (task.IsCanceled)
                        Console.WriteLine("Download cancelled");
                    else
                        Console.WriteLine("Download finished");
                });

                while (true)
                {
                    var s = Console.ReadKey();
                    if (s.KeyChar == 'c')
                    {
                        cts.Cancel(); // if the user presses 'c', cancel the task
                        break;
                    }
                }

                task.Wait(); // wait for the task to finish (Thread.Join())          
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine(e.Message);
                }
                ex.InnerExceptions.ToList().ForEach(e => Console.WriteLine(e.Message));
            }
        }
    }
}