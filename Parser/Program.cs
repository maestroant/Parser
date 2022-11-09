using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Parser
{
    internal class Program
    {

        // глобальные
        public static List<Order> OrdersList = new List<Order>();
        public static List<string> ImeiList = new List<string>();
        public static List<string> WrongList = new List<string>();
        public static List<string> ProxyList = new List<string>();
        public static Setting setting = new Setting("config.json");
        public static string PackId = "0";
        public static Statistics stats = new Statistics();


        static void Main(string[] args)
        {
            Console.WriteLine("Press <Enter> for statistics!");
            // тестовое задание
            //FormatProxy fproxy = new FormatProxy("216.185.47.243:59100:rumyantsev0432:NxCfQwswIU");
            //Ecoatm ecoatm = new Ecoatm("359276661220437", fproxy);

            //Console.WriteLine(ecoatm.Result);
            //Console.ReadKey();


            lock (Program.PackId) Program.PackId = (string)setting.dynamic.PackId;
            lock (ProxyList)
            {
                using (StreamReader reader = File.OpenText("proxy.txt"))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if ((string.IsNullOrEmpty(line)) || (line.Length < 5)) continue;
                        ProxyList.Add(line);
                    }
                }
                Loger.Info("Proxy count: " + ProxyList.Count);
            }


            //WrongList.Add("353103101679145");
            //WrongList.Add("357265099853077");
            //OrdersList.Add(new Order { OrderIMEI = "352107430153731", IMEI1 = "352107430153731" });
            //SendTask();

            // защита пасхалка
            //var date = new DateTime(2022, 12, 6);
            //if (DateTime.Today > date) return;

            Thread t = new Thread(MainThread.Cycle);
            t.Start();


            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey();
                if (c.Key == ConsoleKey.Enter)
                {
                    lock (Program.stats)
                        stats.Show();

                    Console.WriteLine("\nPress <Enter> for statistics!");
                }
            }



            // Console.ReadKey();
        }
    }
}
