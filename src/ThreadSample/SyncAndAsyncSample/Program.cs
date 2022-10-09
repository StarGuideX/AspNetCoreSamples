namespace SyncAndAsyncSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 多线程
            Parallel.For(0, 20, i =>
            {
                // 异步
                //ShowAsync(i.ToString());
                // 同步
                //ShowSync(i.ToString());
            });

            // 单线程
            for (int i = 0; i < 20; i++)
            {
                // ShowSync(i.ToString());
                ShowAsync(i.ToString());
            }



            Console.WriteLine();
        }

        static void ShowSync(string str) 
        {
            Console.WriteLine("******ThreadId: " + Thread.CurrentThread.ManagedThreadId.ToString() + " ******Text: " + str);
            //Thread.Sleep(500);
        }

        static void ShowAsync(string str)
        {
            Task.Run(() => {
                Console.WriteLine(
                    "******ThreadId: " + Thread.CurrentThread.ManagedThreadId.ToString() +
                    "******TaskId: " + Task.CurrentId.ToString() + " ******Text: " + str);
            });
            Task.Delay(500);
        }

    }
}