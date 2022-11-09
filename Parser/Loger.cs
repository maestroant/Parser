using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Parser
{
    internal static class Loger
    {
        static readonly object _locker = new object();

        public static int ErrorCount = 0;

        public static int GetErorCount()
        {
            lock (_locker) return ErrorCount;
        }

        public static void Error(Exception ex)
        {
            Error(ex, null);
        }


        public static void Error(string message)
        {
            Error(null, message);
        }


        public static void Error(Exception ex, string message)
        {
            string logFilePath;

            if (Thread.CurrentThread.Name == null)
            {
                Console.WriteLine(message);      // если поток соновной и индекс потока 0 то показываем лог в консоли
                logFilePath = Path.Combine(@"log", "Error-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            }
            else
                logFilePath = Path.Combine(@"log", "Error-Thread(" + Thread.CurrentThread.Name + ")-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            //var v = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine(message);
            //Console.ForegroundColor = v;

            lock (_locker)
            {
                //File.AppendAllText(logFilePath, string.Format("{0} : {1}\n", DateTime.Now.ToLongTimeString(), message));
                ErrorCount++;
                try
                {
                    using (var writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine("-------------------------------------------------------------------");
                        writer.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
                        writer.WriteLine();

                        if (ex != null)
                        {
                            writer.WriteLine(ex.GetType().FullName);
                            writer.WriteLine("Source : " + ex.Source);
                            writer.WriteLine("Message : " + ex.Message);
                            writer.WriteLine("StackTrace : " + ex.StackTrace);
                            writer.WriteLine("InnerException : " + ex.InnerException?.Message);
                        }

                        if (!string.IsNullOrEmpty(message))
                        {
                            writer.WriteLine(message);
                        }

                        writer.Close();
                    }
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        public static void Info(string message)
        {
            string logFilePath;
            if (Thread.CurrentThread.Name == null)
            {
                Console.WriteLine(message);      // если поток соновной и индекс потока 0 то показываем лог в консоли
                logFilePath = Path.Combine(@"log", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            }
            else
                logFilePath = Path.Combine(@"log", "Thread(" + Thread.CurrentThread.Name + ")-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            //else
            //     logFilePath = Path.Combine(@"log", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            //if (threadIndex == 0)  Console.WriteLine(message);      // если поток соновной и индекс потока 0 то показываем лог в консоли

            if ((int)Program.setting.dynamic.Log == 0) return;      // если логи отключены в файл не пишем

            lock (_locker)
            {
                File.AppendAllText(logFilePath, string.Format("{0} : {1}\n", DateTime.Now.ToLongTimeString(), message));
            }
        }

        public static void StopProgram()
        {
            var v = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            while (true)
            {
                Console.WriteLine("Program is stopped. close the window.");
                Console.ReadKey();
                Process.GetCurrentProcess().Kill();
            }

        }
    }
}
