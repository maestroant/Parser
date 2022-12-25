using System;

namespace Parser
{
    internal class Jobs
    {

        public static void Go()
        {
            // тут вся работа в цикле пока не кончаться задания
            while (true)
            {
                Order order = new Order();

                lock (Program.ImeiList)
                {
                    if (Program.ImeiList.Count == 0) return;
                    order.OrderIMEI = Program.ImeiList[0];
                    Loger.Info("Go: " + order.OrderIMEI);
                    Program.ImeiList.RemoveAt(0);
                }
                RandomProxy randProxy = new RandomProxy();
                FormatProxy fProxy = randProxy.Next();

                //----------------------------------------------------------------------------------------------

                Visible vivible = new Visible(order.OrderIMEI, fProxy);

                if (vivible.Result == Visible.ResultEnum.WRONG_RESULT)
                {
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    Loger.Info("Add Wrong IMEI: " + order.OrderIMEI);
                    continue;
                }

                if (vivible.Result != Visible.ResultEnum.OK)
                {
                    if (vivible.Result == Visible.ResultEnum.ERROR_SOCKS)
                        randProxy.DeleteBadProxy();

                    Loger.Info("Repeat Vivible");
                    fProxy = randProxy.Next();
                    vivible = new Visible(order.OrderIMEI, fProxy);
                }

                //-------------------------------------------------------------------------------------------

                if (vivible.Result != Visible.ResultEnum.OK)
                {
                    Tmobile tmobile = new Tmobile(order.OrderIMEI, fProxy);

                    if (tmobile.Result == Tmobile.ResultEnum.WRONG_RESULT)
                    {
                        lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                        Loger.Info("Add Wrong IMEI: " + order.OrderIMEI);
                        continue;
                    }

                    if (tmobile.Result != Tmobile.ResultEnum.OK)
                    {
                        if (tmobile.Result == Tmobile.ResultEnum.ERROR_SOCKS)
                            randProxy.DeleteBadProxy();
                        Loger.Info("Repeat Tmobile");
                        fProxy = randProxy.Next();
                        tmobile = new Tmobile(order.OrderIMEI, fProxy);
                    }

                    if (tmobile.Result == Tmobile.ResultEnum.WRONG_RESULT)
                    {
                        lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                        Loger.Info("Add Wrong IMEI: " + order.OrderIMEI);
                        continue;
                    }

                    if (string.IsNullOrEmpty(tmobile.IMEI1))
                    {
                        lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                        Loger.Error(order.OrderIMEI + " not found!");
                        continue;
                    }

                    order.IMEI1 = tmobile.IMEI1;
                    order.IMEI2 = tmobile.IMEI2;
                }
                else
                {
                    order.IMEI1 = vivible.IMEI1;
                    order.IMEI2 = vivible.IMEI2;
                }

                //--------------------------------------------------------------------------------------------------

                Business business = new Business(order.OrderIMEI, fProxy);
                if (business.Result != Business.ResultEnum.OK)   //if (vivible.Result != Visible.ResultEnum.OK)
                {
                    if (business.Result == Business.ResultEnum.ERROR_SOCKS)
                        randProxy.DeleteBadProxy();
                    Loger.Info("Repeat Business");
                    fProxy = randProxy.Next();
                    business = new Business(order.OrderIMEI, fProxy);
                }

                if (business.Response == null) continue;

                order.CurrentGSMAStatus = (business.BlackListed == false) ? "Clean" : "Blacklisted";

                //------------------------------------------------------------------------------------------------------

                Ecoatm ecoatm = new Ecoatm(order.OrderIMEI, fProxy);

                if (ecoatm.Result == Ecoatm.ResultEnum.WRONG_RESULT)
                {
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    Loger.Info("Add Wrong IMEI: " + order.OrderIMEI);
                    continue;
                }

                if (ecoatm.StatusCode != 200)
                {
                    if (ecoatm.Result == Ecoatm.ResultEnum.ERROR_SOCKS) randProxy.DeleteBadProxy();
                    Loger.Info("Repeat Ecoatm");
                    fProxy = randProxy.Next();
                    ecoatm = new Ecoatm(order.OrderIMEI, fProxy);
                }

                if (ecoatm.Result == Ecoatm.ResultEnum.WRONG_RESULT)
                {
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    Loger.Info("Add Wrong IMEI: " + order.OrderIMEI);
                    continue;
                }

                if (ecoatm.Response == null)
                {
                    Loger.Info("Response = null");
                    Loger.Info("Add Wrong: " + order.OrderIMEI);
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    continue;
                }

                try
                {
                    order.Model = ecoatm.Response.model + " " + ecoatm.Response.memory + " " + ecoatm.Response.color + " " + ecoatm.Response.anumber;
                    if (order.Model == null) continue;
                    order.SerialNumber = ecoatm.Response.serialNumber;
                    order.FMI = ecoatm.Response.fmip;
                    order.SimLock = ecoatm.Response.simLock;
                    order.Carrier = ecoatm.Response.carrier;
                }
                catch (Exception)
                {
                    Loger.Info("Add Wrong: " + order.OrderIMEI);
                    lock (Program.WrongList) Program.WrongList.Add(order.OrderIMEI);
                    continue;
                }

                lock (Program.OrdersList)
                {
                    Loger.Info("Add Order: " + order.OrderIMEI);
                    Program.OrdersList.Add(order);
                }

            }
        }

    }
}
