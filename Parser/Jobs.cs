using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    internal class Jobs
    {
        public static void Go()
        {
            // тут вся работа в цикле пока не кончаться задания
           // Random rnd = new Random();
           int indexProxy = 0;

            while (true)
            {
                Order order = new Order();
                string proxy;

                lock (Program.ImeiList)
                {
                    if (Program.ImeiList.Count == 0) return;
                    order.OrderIMEI = Program.ImeiList[0];
                    Loger.Info("Go: " + order.OrderIMEI);
                    Program.ImeiList.RemoveAt(0);
                }

                lock (Program.ProxyList)
                {
                    if (Program.ProxyList.Count == 0)
                    {
                        Loger.Error("Proxy list Empty");
                        return;
                    }
                        
                    proxy = Program.ProxyList[indexProxy];
                }

                FormatProxy fProxy = new FormatProxy(proxy);
                indexProxy++;
                lock (Program.OrdersList) if (indexProxy >= Program.OrdersList.Count) indexProxy = 0;

                if (!GetOrder(order, fProxy))
                {
                    lock (Program.ProxyList)
                    {
                       Loger.Info("Info not found. Repeat ride: " + order.OrderIMEI);

                       proxy = Program.ProxyList[indexProxy];
                        FormatProxy fProxy2 = new FormatProxy(proxy);
                        if (!GetOrder(order, fProxy))
                        {
                            lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                            Loger.Info("Wrong Add: " + order.OrderIMEI);
                        } 

                    }

                }

            }
        }

        private static bool GetOrder(Order order, FormatProxy fProxy)
        {
            Visible vivible = new Visible(order.OrderIMEI, fProxy);

            if (string.IsNullOrEmpty(vivible.IMEI1))
            {
                Tmobile tmobile = new Tmobile(order.OrderIMEI, fProxy);
                if (string.IsNullOrEmpty(tmobile.IMEI1))
                {
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    Loger.Error(order.OrderIMEI + " not found!");
                    return false;
                }

                order.IMEI1 = tmobile.IMEI1;
                order.IMEI2 = tmobile.IMEI2;
            }
            else
            {
                order.IMEI1 = vivible.IMEI1;
                order.IMEI2 = vivible.IMEI2;
            }

            Business business = new Business(order.OrderIMEI, fProxy);
            if (business.Response == null) return false;
            order.CurrentGSMAStatus = (business.BlackListed == false) ? "Clean" : "Blacklisted";


            Ecoatm ecoatm = new Ecoatm(order.OrderIMEI, fProxy);
            if (ecoatm.Response == null)
            {
                return false;
            }
            try
            {
                order.Model = ecoatm.Response.model;
                if (order.Model == null) return false;
                order.SerialNumber = ecoatm.Response.serialNumber;
                order.FMI = ecoatm.Response.fmip;
                order.SimLock = ecoatm.Response.simLock;
                order.Carrier = ecoatm.Response.carrier;
            }
            catch (Exception ex)
            {
                //lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                return false;
            }

            lock (Program.OrdersList)
            {
                Loger.Info("Add Order:");
                Loger.Info(order.ToString());
            }

            return true;
        }

    }
}
