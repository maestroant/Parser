using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parser
{
    internal static class Loger
    {
        static readonly object _locker = new object();
        public static void Error(string message)
        {
            string logFilePath = Path.Combine(@"log", "Error-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            var v = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = v;

            lock (_locker)
            {
                File.AppendAllText(logFilePath, string.Format("{0} : {1}\n", DateTime.Now.ToLongTimeString(), message));
            }
        }

        public static void Info(string message)
        {
            string logFilePath = Path.Combine(@"log", DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            Console.WriteLine(message);

            lock (_locker)
            {
                File.AppendAllText(logFilePath, string.Format("{0} : {1}\n", DateTime.Now.ToLongTimeString(), message));
            }
        }
    }
}
