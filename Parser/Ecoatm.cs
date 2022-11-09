using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;

//1.Еайти в хтмл нужный линк на жс :"Payout"}};</ script >< script src = "https://d21pkiizwjo397.cloudfront.net/main.js"
//2.В скрипте найти гуглкей execute-api.us-east-1.amazonaws.com/prod/",s="6LeNZ24gAAAAAFvz_s51P-jdtbYbXOIjH43q3rwH",H="https
//3. Найти в хтмл var OMNI_API_URL = "https://d2xyrw75c4fk9m.cloudfront.net/graphql";


namespace Parser
{
    internal class Ecoatm
    {
        public enum ResultEnum
        {
            OK,
            ERROR_HTTP,    // ошибкаб авторизации, таймат, что угодно
            ERROR_SOCKS,   // ошибка 407 
            ERROR_RESULT,  // ошибка не верный формат отдачи
            WRONG_RESULT   // если модели нет или не та что нужно
        }

        public dynamic Response { get; set; }
        public int StatusCode { get; set; }

        public ResultEnum Result { get; set; }

        public Ecoatm(string imei, FormatProxy proxy = null)
        {
            Loger.Info("\nEcoatm...");

            var cookieContainer = new CookieContainer();
            GetRequest html = new GetRequest("https://www.ecoatm.com/a/devices/apple/iphone/4gb/other");
            if (proxy.Proxy != null)
                html.Proxy = proxy.Proxy;

            html.Referer = "https://www.ecoatm.com/a/devices/apple/iphone/4gb";
            html.Host = "www.ecoatm.com";
            html.Run(cookieContainer);
            StatusCode = html.StatusCode;
            if (string.IsNullOrEmpty(html.Response)) return;

            Uri myUri = new Uri(TextParse.SubString(html.Response, "},\"__typename\":\"Progress\"}};</script><script src=\"", "\""));

            GetRequest mainjs = new GetRequest(myUri.OriginalString);
            if (proxy != null)
                mainjs.Proxy = proxy.Proxy;

            mainjs.Referer = "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other";
            mainjs.Host = myUri.Host;
            mainjs.Run(cookieContainer);
            StatusCode = html.StatusCode;
            if (string.IsNullOrEmpty(mainjs.Response)) return;

            string googleKey = TextParse.SubString(mainjs.Response, "H=\"*Offer valid for qualified devices until 12/31/2022.\",u=\"", "\",X=\"https://");
            string cloudUrl = TextParse.SubString(html.Response, "var OMNI_API_URL = \"", "\";");

            cloudUrl += "/graphql";
            Uri cloudfront = new Uri(cloudUrl);

            if (string.IsNullOrEmpty(googleKey))
            {
                Result = ResultEnum.ERROR_HTTP;
                Loger.Error("googleKey not found!");
                return;
            }



            GetRequest html2 = new GetRequest("https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer");
            if (proxy.Proxy != null)
                html2.Proxy = proxy.Proxy;

            html2.Referer = "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other";
            html2.Host = "www.ecoatm.com";
            html2.Run(cookieContainer);

            if (string.IsNullOrEmpty(html2.Response))
            {
                Result = ResultEnum.ERROR_HTTP;
                Loger.Error("googleKey not found!");
                return;
            }

            string captchaResponse = null;

            for  (int i = 0; i < 5; i++)
            {
                try
                {
                    XevilReCaptcha2 xe = new XevilReCaptcha2((string)Program.setting.dynamic.XevilHost, googleKey, "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer");
                    captchaResponse = xe.Get();
                }
                catch (Exception ex)
                {
                    Loger.Error(ex);
                    //Loger.StopProgram();
                    //return;
                }

                if (captchaResponse == null)
                {
                    Loger.Info("Sleep 5 sec.");
                    Thread.Sleep(5000);
                }
                else
                    break;
            }

            if (captchaResponse == null)
            {
                Loger.Error("Xevil not ready");
                Loger.StopProgram();
            }


            // получаем инфу
            PostRequest post = new PostRequest(cloudfront.OriginalString);
            if (proxy != null)
                post.Proxy = proxy.Proxy;

            post.Referer = "https://www.ecoatm.com/";
            post.Host = cloudfront.Host;
            post.ContentType = "application/json";
            post.Accept = "*/*";
            post.Headers.Add("Sec-Fetch-Dest", "empty");
            post.Headers.Add("Sec-Fetch-Mode", "cors");
            post.Headers.Add("Sec-Fetch-Site", "cross-site");
            //post.Headers.Add("sec-gpc", "1");
            post.Headers.Add("TE", "trailers");

            post.Useragent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            post.Data = "{\"operationName\":\"verifyIMEINumber\",\"variables\":{\"input\":{\"imeiNumber\":\"" + imei
                + "\",\"deviceBrand\":\"Apple\",\"financeLock\":\"no\",\"recaptchaToken\":\"" + captchaResponse
                + "\"}},\"query\":\"mutation verifyIMEINumber($input: VerifyIMEINumberInput) {\\n  verifyIMEINumber(input: $input) {\\n    data\\n    __typename\\n  }\\n}\\n\"}";

            post.Run(cookieContainer);
            StatusCode = post.StatusCode;


            if (StatusCode == 407)
            {
                Result = ResultEnum.ERROR_SOCKS;
                return;
            }

            if (StatusCode != 200)
            {
                Result = ResultEnum.ERROR_HTTP;
                return;
            }
            if (string.IsNullOrEmpty(post.Response))
            {
                Result = ResultEnum.ERROR_HTTP;
                return;
            }

            if (post.Response != null)
            {
                Loger.Info("Log Response:\n" + post.Response);

                if (post.Response.IndexOf("error", StringComparison.CurrentCultureIgnoreCase) > 0)
                {
                    Result = ResultEnum.ERROR_RESULT;
                    return;
                }

                try
                {
                    JObject obj = JObject.Parse(post.Response);
                    Response = JObject.Parse(obj["data"]["verifyIMEINumber"]["data"].ToString());
                    string brand = (string)Response.brand;
                    if (post.Response.IndexOf("APPLE", StringComparison.CurrentCultureIgnoreCase) < 0)
                    {
                        Result = ResultEnum.WRONG_RESULT;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Loger.Error(ex, "Ecoatm JSON");
                    Result = ResultEnum.ERROR_RESULT;
                    return;
                }
            }
        }
    }
}


//"data": {
//    "verifyIMEINumber": {
//        "data": "{\"model\":\"iPhone 11 Pro Max\",\"modelNumber\":null,\"anumber\":\"A2161\",\"brand\":\"APPLE\",\"intendedCarrier\":\"ATT/TMO/VZW\",\"serialNumber\":\"F2LDD7PUN70G\",\"simLock\":\"Locked\",\"carrier\":\"AT&T\",\"memory\":\"64GB\",\"color\":\"Midnight Green\",\"financeLock\":\"NO\",\"fmip\":\"ON\",\"apiStatus\":\"API Check 1210 OK\"}",
//    "__typename": "VerifyIMEINumberResult"
//    }
//}
