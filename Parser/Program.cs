using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Remote;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


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

        static void Main(string[] args)
        {

            //ProxyList.Add("216.185.47.243:59100:rumyantsev0432:NxCfQwswIU");

            ProxyList = File.ReadLines("proxy.txt").ToList();
            for (int i = 0; i < ProxyList.Count; i++)
            {
                if (ProxyList[i].Length < 5) ProxyList.RemoveAt(i);
            }
            Loger.Info("Proxy count: " + ProxyList.Count);

            Adminka adminka = new Adminka((string)setting.dynamic.LoginAdmin, (string)setting.dynamic.PassAdmin);
            adminka.Signin();
            adminka.GetInProcess(2);

            var date = new DateTime(2023, 1, 1);

            if (DateTime.Today.AddMonths(1) < date)
            {
                Jobs.Go();
            }

            lock (WrongList)
            {
                if(WrongList.Count > 0)
                {
                    TextWriter tw = new StreamWriter("WrongList.txt");
                    foreach (String s in WrongList)
                        tw.WriteLine(s);
                    tw.Close();
                    WrongList.Clear();
                }
            }

            if (OrdersList.Count < 1)
            {
                Loger.Info("Orders not found!");
                Console.ReadKey();
                return;
            }

            adminka.Signin();
            if (adminka.SendTask()) lock (OrdersList) OrdersList.Clear();

            // adminka.GetInProcess(100);

                    //string s = "{\r\n   \"data\":{\r\n      \"verifyIMEINumber\":{\r\n         \"data\":\"{\\\"model\\\":\\\"iPhone 11 Pro Max\\\",\\\"modelNumber\\\":null,\\\"anumber\\\":\\\"A2161\\\",\\\"brand\\\":\\\"APPLE\\\",\\\"intendedCarrier\\\":\\\"ATT/TMO/VZW\\\",\\\"serialNumber\\\":\\\"F2LDD7PUN70G\\\",\\\"simLock\\\":\\\"Locked\\\",\\\"carrier\\\":\\\"AT&T\\\",\\\"memory\\\":\\\"64GB\\\",\\\"color\\\":\\\"Midnight Green\\\",\\\"financeLock\\\":\\\"NO\\\",\\\"fmip\\\":\\\"ON\\\",\\\"apiStatus\\\":\\\"API Check 1210 OK\\\"}\",\r\n         \"__typename\":\"VerifyIMEINumberResult\"\r\n      }\r\n   }\r\n}";

                    //JObject obj = JObject.Parse(s);
                    //JObject obj2 = JObject.Parse(obj["data"]["verifyIMEINumber"]["data"].ToString());
                    //dynamic din = obj2;

                    //Console.WriteLine(obj2["model"].ToString());
                    //Console.WriteLine(din.model);

                    //Console.ReadKey();

            //Console.WriteLine("Press Esc to exit");

            //while (true)
            //{
            //    if (Console.ReadKey(true).Key == ConsoleKey.S)
            //    {
            //        Statistics.Get();
            //    }

            //    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
            //    {
            //        Process.GetCurrentProcess().Kill();
            //    }
            //} 


            //ImeiList.Add("352844115151198");
            //ImeiList.Add("354741664094654");
            //ImeiList.Add("357337095223512");





            Console.WriteLine("End.");

            Console.ReadKey();
        }
    }
}
