using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Parser
{


    internal class Adminka
    {
        // https://www.unlock-lock.com/LockPanelU/login
        public string Login { get; set; }
        public string Pass { get; set; }
        CookieContainer cookieContainer { get; set; }
        public string Html1 { get; set; }
        //private List<string> itemStr = new List<string>();  // временное хранение сырых заданий

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
            postRequest.Run(cookieContainer);

            if (string.IsNullOrEmpty(postRequest.Response))
            {
                Loger.Error("The page is not available");
                return false;
            }

            if ((postRequest.Response.IndexOf("The username or password is incorrect")) != -1)
            {
                //The username or password is incorrect
                Loger.Error("The username or password is incorrect");
                return false;
            }

            Html1 = postRequest.Response;

            Loger.Info("OK");
            return true;
        }


        // Получить нужное колличество тасков 
        // return: колличество тасков, -1 ошибка. 
        public int GetNew(int count = 100)
        {
            Loger.Info("GetTask...");
            if (string.IsNullOrEmpty(Html1)) return -1;
            string newImeiOrders =
                TextParse.SubString(Html1, "<a  class=\"navbar-nav-link dropdown-toggle",
                "href=\"https://", "\">New IMEI Orders</a>");

            if (string.IsNullOrEmpty(newImeiOrders)) return -1; // Невозможно найти ссылку

            GetRequest getRequest = new GetRequest("https://" + newImeiOrders);
            //getRequest.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest.Accept = "*/*";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest.Referer = "https://www.unlock-lock.com/LockPanelU/dashboard";
            getRequest.Host = "www.unlock-lock.com";
            getRequest.Run(cookieContainer);

            if (string.IsNullOrEmpty(getRequest.Response))
                return -1;

            string testCarrierCheck;
            lock (Program.PackId)
                testCarrierCheck = GetUrlByPakid(getRequest.Response, Program.PackId);

            if (string.IsNullOrEmpty(testCarrierCheck))
            {
                Loger.Info("No required packid");
                return 0; // Невозможно найти ссылку
            }

            GetRequest getRequest2 = new GetRequest(testCarrierCheck);
            //getRequest2.Proxy = new WebProxy("127.0.0.1:8888");
            getRequest2.Accept = "*/*";
            getRequest2.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest2.Referer = newImeiOrders;
            getRequest2.Host = "www.unlock-lock.com";
            getRequest2.Run(cookieContainer);

            if (string.IsNullOrEmpty(getRequest2.Response))
                return -1;

            List<string> itemStr = new List<string>();
            itemStr = Parse(getRequest2.Response, count);
            if (itemStr.Count == 0) return -1;
            AddItemToImei(itemStr);

            lock (Program.ImeiList) if (Program.ImeiList.Count() == 0)
                {
                    // Не получили оредров
                    Loger.Info("Orders not founf!");
                    return 0;
                }

            // Запрос что бы отчитатся за полученые оридера
            //https://www.unlock-lock.com/LockPanelU/services/codes?packId=4605&codeStatusId=1&pT=1&searchType=1&txtFromDt=&oB=DESC&rms=0
            string testGetOrders = TextParse.SubString(getRequest2.Response,
                "<div class=\"card-body\">", "<form action=\"", "\" name=");
            if (string.IsNullOrEmpty(testGetOrders)) return -1; // Невозможно найти ссылку

            PostRequest postRequest = new PostRequest(testGetOrders);

            string param = "";
            foreach (var item in itemStr)
            {
                param += WebUtility.UrlEncode("chkCodes[]") + "=" + WebUtility.UrlEncode(item) + "&";
            }

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

            if (string.IsNullOrEmpty(postRequest.Response))
                return -1;

            if (postRequest.StatusCode == 200)
            {
                lock (Program.ImeiList)
                {
                    Loger.Info("Orders count = " + Program.ImeiList.Count);
                    return Program.ImeiList.Count;
                }

            }

            return -1;
        }


        public int GetInProcess(int count = 100)
        {
            Loger.Info("GetInProcess...");
            if (string.IsNullOrEmpty(Html1)) return -1;
            string newImeiOrders =
                   TextParse.SubString(Html1, "<a  class=\"navbar-nav-link dropdown-toggle",
                   ">New IMEI Orders</a><a class=\"dropdown-item\" href=\"https://", "\">In Process IMEI Orders");

            if (string.IsNullOrEmpty(newImeiOrders)) return -1; // Невозможно найти ссылку

            GetRequest getRequest = new GetRequest("https://" + newImeiOrders);
            getRequest.Accept = "*/*";
            getRequest.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest.Referer = "https://www.unlock-lock.com/LockPanelU/dashboard";
            getRequest.Host = "www.unlock-lock.com";
            getRequest.Run(cookieContainer);

            if (string.IsNullOrEmpty(getRequest.Response))
                return -1;

            string testCarrierCheck;
            try
            {
                lock (Program.PackId)
                {
                    testCarrierCheck = GetUrlByPakid(getRequest.Response, Program.PackId);
                }
            }
            catch (Exception ex)
            {
                Loger.Error(ex, "GetUrlByPakid");
                return -1;
            }


            if (string.IsNullOrEmpty(testCarrierCheck))
            {
                Loger.Info("No required packid");
                return 0; // Невозможно найти ссылку
            }

            GetRequest getRequest2 = new GetRequest(testCarrierCheck);
            getRequest2.Accept = "*/*";
            getRequest2.Useragent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            getRequest2.Referer = newImeiOrders;
            getRequest2.Host = "www.unlock-lock.com";
            getRequest2.Run(cookieContainer);

            if (string.IsNullOrEmpty(getRequest2.Response))
                return -1;

            List<string> itemStr = new List<string>();
            itemStr = Parse(getRequest2.Response, count);
            if (itemStr.Count == 0) return -1;
            AddItemToImei(itemStr);


            lock (Program.ImeiList) if (Program.ImeiList.Count() == 0)
                {
                    // Не получили оредров
                    Loger.Info("No orders InProcess received");
                    return 0;
                }

            lock (Program.ImeiList)
            {
                Loger.Info("Orders count = " + Program.ImeiList.Count);
                return Program.ImeiList.Count;
            }

        }


        public bool SendOrders()
        {
            Loger.Info("SendTask...");

            PostRequest postRequest = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
            string param = FormatResult();
            if (string.IsNullOrEmpty(param)) return false;

            try
            {
                string s = WebUtility.UrlEncode(param);
                s = s.Replace("!", "%21");
                lock (Program.PackId) postRequest.Data = "txtMultiIMEIs=" + s + $"&searchType=3&bulk=1&packId={Program.PackId}&replyType=0&replySep=%21";
                postRequest.Accept = "*/*";
                postRequest.Host = "www.unlock-lock.com";
                postRequest.ContentType = "application/x-www-form-urlencoded";
                postRequest.Run(cookieContainer);

                if (string.IsNullOrEmpty(postRequest.Response))
                    return false;

                //File.WriteAllText("postRequest.html", postRequest.Response);
                List<string> itemStr = new List<string>();
                itemStr = Parse(postRequest.Response, 100);

                if (itemStr.Count < 1)
                {
                    Loger.Error("Orders not found!");
                    return false;
                }

                Loger.Info("Orders count: " + itemStr.Count);

                s = "txtBulkCodeVal=&hdsbmt=&" + FormatParamForSend(itemStr) + $"&oB=DESC&applyAPI=0&cldFrm=6&uId=0&bulk=1&searchType=3&codeStatusId=0&hdsbmt=&printFields=0&addNL=0&packId=" +
                    $"{Program.PackId}&txtFromDt=&packageId=0&codeStatusId=0&txtBulkIMEIs=&txtUName=&records={itemStr.Count}&start=0&sc=0&pT=0&rm=0&rms=0";

                PostRequest postRequest2 = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
                lock (Program.PackId) postRequest2.Data = "txtMultiIMEIs=" + s + $"&searchType=3&bulk=1&packId={Program.PackId}&replyType=0&replySep=%21";
                postRequest2.Accept = "*/*";
                postRequest2.Host = "www.unlock-lock.com";
                postRequest2.ContentType = "application/x-www-form-urlencoded";
                postRequest2.Run(cookieContainer);

                if (string.IsNullOrEmpty(postRequest2.Response))
                    return false;

                //File.WriteAllText("getRequest.html", postRequest.Response);

                return (postRequest2.StatusCode == 200) ? true : false;

            }
            catch (Exception ex)
            {
                Loger.Error(ex, "SendTask");
            }

            Loger.Error("Failed to send orders");

            return false;
        }


        public bool SendWrong()
        {

            Loger.Info("SendWrong...");
            lock (Program.WrongList) if (Program.WrongList.Count == 0)
                {
                    Loger.Info("Wrong list Count = 0");
                    return true;
                }

            PostRequest postRequest = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
            string param = FormatWrong();
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

                if (string.IsNullOrEmpty(postRequest.Response))
                    return false;
                List<string> itemStr = new List<string>();
                itemStr = Parse(postRequest.Response, 100);

                if (itemStr.Count < 1)
                {
                    Loger.Error("Orders not found!");
                    return false;
                }

                Loger.Info("Wrong count: " + itemStr.Count);

                param = "";
                for (int i = 0; i < itemStr.Count; i++)
                {
                    // &chkReply%5B%5D=4377632%7C100510%7C0.5%7C353889100089829%7C0%7C4605%7C1&txtCode0=Wrong_imei&chkCodes%5B%5D=4377632%7C100510%7C0.5%7C353889100089829%7C0%7C4605%7C1 
                    param += "chkReply%5B%5D=" + WebUtility.UrlEncode(itemStr[i]) + "&txtCode" + i + "="
                        + WebUtility.UrlEncode("Wrong_imei") + "&chkCodes%5B%5D=" + WebUtility.UrlEncode(itemStr[i]) + "&";
                }

                s = "txtBulkCodeVal=&hdsbmt=&" + param + $"oB=DESC&applyAPI=0&cldFrm=6&uId=0&bulk=1&searchType=3&codeStatusId=0&hdsbmt=&printFields=0&addNL=0&packId=" +
                    $"{Program.PackId}&txtFromDt=&packageId=0&codeStatusId=0&txtBulkIMEIs=&txtUName=&records={itemStr.Count}&start=0&sc=0&pT=0&rm=0&rms=0";


                PostRequest postRequest2 = new PostRequest("https://www.unlock-lock.com/LockPanelU/services/codes?frmId=101&fTypeId=16?frmId=101&fTypeId=16");
                lock (Program.PackId) postRequest2.Data = s;

                postRequest2.Accept = "*/*";
                postRequest2.Host = "www.unlock-lock.com";
                postRequest2.ContentType = "application/x-www-form-urlencoded";
                postRequest2.Run(cookieContainer);

                if (string.IsNullOrEmpty(postRequest2.Response))
                    return false;

                //File.WriteAllText("getRequest.html", postRequest.Response);

                return (postRequest2.StatusCode == 200) ? true : false;

            }
            catch (Exception ex)
            {
                Loger.Error(ex, "SendWrong");
            }

            Loger.Error("Failed to send orders");

            return true;
        }


        private void AddItemToImei(List<string> itemStr)
        {
            foreach (string item in itemStr)
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


        private void ParseItem(string tr, List<string> itemStr)
        {
            if (string.IsNullOrEmpty(tr)) return;

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

            itemStr.Add(tr.Substring(strtIndex, endIndex - strtIndex));
        }


        // html - текст страницы для парсинга, count - количестов нужных ордеров которе ищем
        private List<string> Parse(string html, int count)
        {
            List<string> itemStr = new List<string>();
            if (string.IsNullOrEmpty(html)) return itemStr;

            string s = TextParse.SubString(html, "<tbody>", "</tbody>");

            int strtIndex = 0;
            int endIndex = 0;

            for (int i = 0; i < count; i++)
            {
                if ((strtIndex = s.IndexOf("<tr>", strtIndex)) < 0) break;
                if ((endIndex = s.IndexOf("</tr>", strtIndex)) < 0) break;
                strtIndex += 4;
                ParseItem(s.Substring(strtIndex, endIndex - strtIndex), itemStr);
                strtIndex = endIndex + 5;
            }

            return itemStr;
        }



        // Находит нужную строку задания с заданным паид. возвращает url
        private string GetUrlByPakid(string html, string packId)
        {
            try
            {
                if ( (string.IsNullOrEmpty(html)) || (string.IsNullOrEmpty(packId)) )  return null;
                string tbody = TextParse.SubString(html, "<tbody>", "</tbody>");

                int strtIndex = 0;
                int endIndex = 0;

                while (true)
                {
                    if ((strtIndex = tbody.IndexOf("<tr>", strtIndex)) < 0) break;
                    if ((endIndex = tbody.IndexOf("</tr>", strtIndex)) < 0) break;
                    strtIndex += 4;
                    string s = tbody.Substring(strtIndex, endIndex - strtIndex);

                    int si = s.IndexOf("<a title=\"Take Action\" href=\"");
                    if (si < 0)
                    {
                        // ошибка ParseItem
                        return null;
                    }
                    si += 29;

                    int ei = s.IndexOf("\"", si);
                    if (si < 0)
                    {
                        // ошибка ParseItem
                        return null;
                    }

                    string url = s.Substring(si, ei - si);
                    if (string.IsNullOrEmpty(url)) return null;
                    string id = TextParse.SubString(url, "packId=", "&");

                    if (packId == id)
                        return url;

                    strtIndex = endIndex + 5;
                }
            }
            catch (Exception ex)
            {
                Loger.Error(ex);
                return null;
            }

            return null;
        }

        // все ордера Program.OrdersList  привести в строку 
        private string FormatResult()
        {
            string s = "";

            lock (Program.OrdersList)
            {
                if (Program.OrdersList.Count == 0) return null;

                foreach (var orders in Program.OrdersList)
                {
                    s += orders.OrderIMEI + "!Model: " + orders.Model + " <br>IMEI1: " + orders.IMEI1;
                    if (orders.IMEI2 != null) s += " <br>IMEI2: " + orders.IMEI2;
                    s += " <br>Serial Number: " + orders.SerialNumber + " <br>FMI: " + orders.FMI + "<br>Current GSMA Status: " + orders.CurrentGSMAStatus +
                        " <br>Carrier: " + orders.Carrier + " <br>SimLock Status: " + orders.SimLock + "\r\n";
                }

            }

            return s;
        }

        private string FormatParamForSend(List<string> itemStr)
        {
            string param = "";
            lock (Program.OrdersList)
            {

                for (int i = 0; i < itemStr.Count; i++)
                {
                    string formatRaw = "";
                    // подобрать нужны ИД для записи
                    for (int j = 0; j < Program.OrdersList.Count; j++)
                    {
                        if (itemStr[i].IndexOf(Program.OrdersList[j].OrderIMEI) > -1)
                        {
                            formatRaw = "Model: " + Program.OrdersList[j].Model + " <br>IMEI1: " + Program.OrdersList[j].IMEI1;
                            if (Program.OrdersList[j].IMEI2 != null) formatRaw += " <br>IMEI2: " + Program.OrdersList[j].IMEI2;
                            formatRaw += " <br>Serial Number: " + Program.OrdersList[j].SerialNumber +
                                " <br>FMI: " + Program.OrdersList[j].FMI + "<br>Current GSMA Status: " + Program.OrdersList[j].CurrentGSMAStatus +
                                                   " <br>Carrier: " + Program.OrdersList[j].Carrier + " <br>SimLock Status: " + Program.OrdersList[j].SimLock + "\r\n";
                            break;
                        }
                    }

                    Loger.Info(formatRaw);
                    param += "chkReply%5B%5D=" + WebUtility.UrlEncode(itemStr[i]) + "&txtCode" + i + "=" + WebUtility.UrlEncode(formatRaw) + "&";
                }

            }

            return param;
        }


        // все ордера Program.WrongList  привести в строку 
        private string FormatWrong()
        {
            string s = "";

            lock (Program.WrongList)
            {
                if (Program.WrongList.Count == 0) return null;

                for (int i = 0; i < Program.WrongList.Count; i++)
                {
                    s += Program.WrongList[i] + "!Wrong_imei\r\n";
                }

            }

            return s;
        }

    }
}
