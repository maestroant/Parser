using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Parser.Program;
using static System.Net.Mime.MediaTypeNames;

namespace Parser
{


    internal class Adminka
    {
        // https://www.unlock-lock.com/LockPanelU/login
        public string Login { get; set; }
        public string Pass { get; set; }
        CookieContainer cookieContainer { get; set; }
        public string Html1 { get; set; }
        private List<string> itemStr = new List<string>();  // временное хранение сырых заданий

        public Adminka(string login, string pass)
        {
            Login = WebUtility.UrlEncode(login);
            Pass = WebUtility.UrlEncode(pass);
        }

        public bool Signin()
        {
            Loger.Info("Signin...");
            cookieContainer = new CookieContainer();
            
            PostRequest postRequest = new PostRequest("https://www.unlock-lock.com/LockPanelU/login");
            postRequest.Data = $"username={Login}&url=&password={Pass}";
            postRequest.Accept = "*/*";
            postRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            postRequest.ContentType = "application/x-www-form-urlencoded";
            postRequest.Referer = "https://www.unlock-lock.com/LockPanelU/login";
            postRequest.Host = "www.unlock-lock.com";
            //postRequest.Proxy = new WebProxy("127.0.0.1:8888");
            postRequest.Run(cookieContainer);
           

            if ((postRequest.Response.IndexOf("The username or password is incorrect")) != -1)
            {
                //The username or password is incorrect
                return false;
            }

            Html1 = postRequest.Response;

            Loger.Info("OK");
            return true;
        }


        // Получить нужное колличество тасков
        public bool GetTask(int count = 100)
        {
            Loger.Info("GetTask...");

            string newImeiOrders = 
                TextParse.SubString(Html1, "<a  class=\"navbar-nav-link dropdown-toggle", 
                "href=\"https://", "\">New IMEI Orders</a>");

            if (string.IsNullOrEmpty(newImeiOrders)) return false; // Невозможно найти ссылку

            GetRequest getRequest = new GetRequest("https://" + newImeiOrders);
            //getRequest.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Accept = "*/*";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest.Referer = "https://www.unlock-lock.com/LockPanelU/dashboard";
            getRequest.Host = "www.unlock-lock.com";
            getRequest.Run(cookieContainer);

            string testCarrierCheck = TextParse.SubString(getRequest.Response, 
                "<td>TEST CARRIER CHECK", "<a title=\"Take Action\" href=\"", "\"><i class=");
            if (string.IsNullOrEmpty(testCarrierCheck)) return false; // Невозможно найти ссылку

            GetRequest getRequest2 = new GetRequest(testCarrierCheck);
            //getRequest2.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest2.Accept = "*/*";
            getRequest2.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest2.Referer = newImeiOrders;
            getRequest2.Host = "www.unlock-lock.com";
            getRequest2.Run(cookieContainer);

            Parse(getRequest2.Response, count);
            AddItemToImei();

            lock (Program.ImeiList) if (Program.ImeiList.Count() == 0)
            {
                // Не получили оредров
                Loger.Error("Orders not founf!");
                return false;
            }

            // Запрос что бы отчитатся за полученые оридера
            //https://www.unlock-lock.com/LockPanelU/services/codes?packId=4605&codeStatusId=1&pT=1&searchType=1&txtFromDt=&oB=DESC&rms=0
            string testGetOrders = TextParse.SubString(getRequest2.Response,
                "<div class=\"card-body\">", "<form action=\"", "\" name=");
            if (string.IsNullOrEmpty(testGetOrders)) return false; // Невозможно найти ссылку

            PostRequest postRequest = new PostRequest(testGetOrders);
            lock (Program.PackId) Program.PackId = TextParse.SubString(testGetOrders, "packId=", "&");
            //postRequest.Proxy = new WebProxy("127.0.0.1:8888");

            string param = "";
            foreach (var item in itemStr)
            {
                param += WebUtility.UrlEncode("chkCodes[]") + "=" + WebUtility.UrlEncode(item) + "&";
            }

            itemStr.Clear();

            postRequest.Data = $"hdsbmt=&{param}oB=DESC&applyAPI=0&cldFrm=4&uId=0&bulk=1&searchType=1&codeStatusId=1&hdsbmt=&printFields=0&addNL=0&packId={Program.PackId}&txtFromDt=&packageId=0&codeStatusId=1&txtBulkIMEIs=&txtUName=&records=100&start=0&sc=0&pT=1&rm=0&rms=0";
            postRequest.Accept = "*/*";
            postRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            postRequest.Referer = testCarrierCheck;
            postRequest.Host = "www.unlock-lock.com";
            postRequest.Headers.Add("Sec-Fetch-Site", "same-origin");
            postRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            postRequest.Headers.Add("Sec-Fetch-User", "?1");
            postRequest.Headers.Add("Sec-Fetch-Dest", "document");
            postRequest.Headers.Add("Origin", "https://www.unlock-lock.com");
            postRequest.Headers.Add("DNT", "1");
            postRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
            postRequest.ContentType = "application/x-www-form-urlencoded";
            postRequest.Run(cookieContainer);

            if (postRequest.StatusCode == 200)
            {
                lock (Program.ImeiList) Loger.Info("Orders count = " + Program.ImeiList.Count);
                return true;
            }

            return false;
        }


        public bool GetInProcess(int count = 100)
        {
            Loger.Info("GetInProcess...");

            string newImeiOrders =
                   TextParse.SubString(Html1, "<a  class=\"navbar-nav-link dropdown-toggle",
                   ">New IMEI Orders</a><a class=\"dropdown-item\" href=\"https://", "\">In Process IMEI Orders");

            if (string.IsNullOrEmpty(newImeiOrders)) return false; // Невозможно найти ссылку

            GetRequest getRequest = new GetRequest("https://" + newImeiOrders);
            //getRequest.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Accept = "*/*";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest.Referer = "https://www.unlock-lock.com/LockPanelU/dashboard";
            getRequest.Host = "www.unlock-lock.com";
            getRequest.Run(cookieContainer);

            string testCarrierCheck = TextParse.SubString(getRequest.Response,
                "TEST CARRIER CHECK", "<a title=\"Take Action\" href=\"", "\"><i class=");
            if (string.IsNullOrEmpty(testCarrierCheck)) return false; // Невозможно найти ссылку

            lock (Program.PackId) Program.PackId = TextParse.SubString(testCarrierCheck, "packId=", "&");

            GetRequest getRequest2 = new GetRequest(testCarrierCheck);
            //getRequest2.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest2.Accept = "*/*";
            getRequest2.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest2.Referer = newImeiOrders;
            getRequest2.Host = "www.unlock-lock.com";
            getRequest2.Run(cookieContainer);

            Parse(getRequest2.Response, count);
            AddItemToImei();
            itemStr.Clear();

            lock (Program.ImeiList) if (Program.ImeiList.Count() == 0)
            {
                // Не получили оредров
                Loger.Error("no jobs received");
                return false;
            }

            lock (Program.ImeiList) Loger.Info("Orders count = " + Program.ImeiList.Count);

            return true;
        }


        public bool SendTask()
        {

            Loger.Info("SendTask...");  

            PostRequest postRequest = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
            string param = FormatResult();
            if (string.IsNullOrEmpty(param)) return false;

            try
            {
                string s = WebUtility.UrlEncode(param);
                s = s.Replace("!", "%21");
                lock (Program.PackId) postRequest.Data = "txtMultiIMEIs=" + s + $"&searchType=3&bulk=1&packId={Program.PackId}&replyType=1&replySep=%21";
                postRequest.Accept = "*/*";
                postRequest.Host = "www.unlock-lock.com";
                postRequest.ContentType = "application/x-www-form-urlencoded";
                postRequest.Run(cookieContainer);

                File.WriteAllText("postRequest.html", postRequest.Response);

                Parse(postRequest.Response, 100);
                if (itemStr.Count < 1)
                {
                    Loger.Error("Orders not found!");
                    return false;
                }

                Loger.Info("Orders count: " + itemStr.Count);

                param = "";
                for (int i = 0; i < itemStr.Count; i++)
                {
                    // chkReply[]=4249453|100510|0.5|353904102795743|0|4605|1
                    param += "chkReply%5B%5D=" + WebUtility.UrlEncode(itemStr[i]) + "&txtCode" + i + "=" + WebUtility.UrlEncode(FormatRawToParam(i)) + "&";
                }

                s = "txtBulkCodeVal=&hdsbmt=&" + param + $"&oB=DESC&applyAPI=0&cldFrm=6&uId=0&bulk=1&searchType=3&codeStatusId=0&hdsbmt=&printFields=0&addNL=0&packId=" +
                    $"{Program.PackId}&txtFromDt=&packageId=0&codeStatusId=0&txtBulkIMEIs=&txtUName=&records={itemStr.Count}&start=0&sc=0&pT=0&rm=0&rms=0";


                PostRequest postRequest2 = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
                lock (Program.PackId) postRequest2.Data = "txtMultiIMEIs=" + s + $"&searchType=3&bulk=1&packId={Program.PackId}&replyType=1&replySep=%21";
                postRequest2.Accept = "*/*";
                postRequest2.Host = "www.unlock-lock.com";
                postRequest2.ContentType = "application/x-www-form-urlencoded";
                postRequest2.Run(cookieContainer);

                File.WriteAllText("getRequest.html", postRequest.Response);

                return (postRequest2.StatusCode == 200) ? true : false;

            }
            catch ( Exception)
            {
                //
            }

            // если не вышло отправить сохранить в файл
            try
            {
                Loger.Error("Failed to send orders");
                File.AppendAllText("log\\NoSendOrders.txt", param);
            }
            catch (Exception ex)
            {
                Loger.Error("Error save to file log\\NoSendOrders.txt" + " : " + ex.GetType().FullName);
            }

            return false;
        }


        private void AddItemToImei()
        {
            foreach(string item in itemStr)
            {
                int strtIndex, endIndex;

                if ((endIndex = item.IndexOf("|")) < 0)
                {
                    // ошибка ParseItem
                    return;
                }

                strtIndex = item.IndexOf("|", endIndex + 1);
                strtIndex = item.IndexOf("|", strtIndex + 1);
                endIndex = item.IndexOf("|", strtIndex + 1);
                strtIndex += 1;
                lock (Program.ImeiList) Program.ImeiList.Add(item.Substring(strtIndex, endIndex - strtIndex));
            }
        }


        private void ParseItem(string tr)
        {

            int strtIndex = tr.IndexOf("value=\"");
            if (strtIndex < 0)
            {
                // ошибка ParseItem
                return;
            }

            strtIndex += 7;

            int endIndex = tr.IndexOf("\"", strtIndex);
            if (strtIndex < 0)
            {
                // ошибка ParseItem
                return;
            }

            itemStr.Add( tr.Substring(strtIndex, endIndex - strtIndex));
        }


        // html - текст страницы для парсинга, count - количестов нужных ордеров которе ищем
        private void Parse(string html, int count)
        {
            string s = TextParse.SubString(html, "<tbody>", "</tbody>");

            int strtIndex = 0;
            int endIndex = 0;

            for (int i = 0; i < count; i++)
            {
                if ((strtIndex = s.IndexOf("<tr>", strtIndex)) < 0) break;
                if ((endIndex = s.IndexOf("</tr>", strtIndex)) < 0) break;
                strtIndex += 4;
                ParseItem(s.Substring(strtIndex, endIndex - strtIndex));
                strtIndex = endIndex + 5;
            }

        }

        private string FormatResult()
        {
            string s = "";

            lock (Program.OrdersList)
            {
                if (Program.OrdersList.Count == 0) return null;

                foreach (var orders in Program.OrdersList)
                {
                    s += orders.OrderIMEI + "!Model: " + orders.Model + " <br>IMEI1: " + orders.IMEI1 + " <br>IMEI2: " + orders.IMEI2 +
                        " <br>Serial Number: " + orders.SerialNumber + " <br>FMI: " + orders.FMI + " Current GSMA Status: " + orders.CurrentGSMAStatus +
                        " <br>Carrier: " + orders.Carrier + " <br>SimLock Status: " + orders.SimLock + "\r\n";
                }

            }
            return s;
        }

        private string FormatRawToParam(int index)
        {
            lock (Program.OrdersList)
            {
                return "Model: " + Program.OrdersList[index].Model + " <br>IMEI1: " + Program.OrdersList[index].IMEI1 +
                    " <br>IMEI2: " + Program.OrdersList[index].IMEI2 + " <br>Serial Number: " + Program.OrdersList[index].SerialNumber +
                    " <br>FMI: " + Program.OrdersList[index].FMI + " Current GSMA Status: " + Program.OrdersList[index].CurrentGSMAStatus +
                                       " <br>Carrier: " + Program.OrdersList[index].Carrier + " <br>SimLock Status: " + Program.OrdersList[index].SimLock + "\r\n";
            }

        }

    }
}
