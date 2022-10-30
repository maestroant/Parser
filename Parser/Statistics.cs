using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class Statistics
    {
        private static System.DateTime dateStart = DateTime.Now;

        private static void GreenWriteln(string name, string value)
        {
            var v = Console.ForegroundColor;
            Console.Write(name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            Console.ForegroundColor = v;
        }

        private static void YellowWriteln(string name, string value)
        {
            var v = Console.ForegroundColor;
            Console.Write(name);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            Console.ForegroundColor = v;
        }

        private static void RedWriteln(string name, string value)
        {
            var v = Console.ForegroundColor;
            Console.Write(name);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(value);
            Console.ForegroundColor = v;
        }

        public static void Get()
        {
            System.TimeSpan date = DateTime.Now - dateStart;
            int i;
            Console.WriteLine("======================================================");
            Console.WriteLine("   Number of Tasks: \t\t123" );
            Console.WriteLine("   Number of Socks: \t\t2343");
            YellowWriteln("   Start time: \t\t\t", dateStart.ToString());
            GreenWriteln("   Time has Passed: \t\t", date.ToString(@"hh\:mm\:ss"));
            RedWriteln("   Number of active Threads: \t", Process.GetCurrentProcess().Threads.Count.ToString());
            lock (Program.OrdersList) i = Program.OrdersList.Count;
            GreenWriteln("   Number of processed Orders: \t", i.ToString());
            Console.WriteLine("");
        }
    }
}


