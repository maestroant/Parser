using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//1.Еайти в хтмл нужный линк на жс :"Payout"}};</ script >< script src = "https://d21pkiizwjo397.cloudfront.net/main.js"
//2.В скрипте найти гуглкей execute-api.us-east-1.amazonaws.com/prod/",s="6LeNZ24gAAAAAFvz_s51P-jdtbYbXOIjH43q3rwH",H="https
//3. Найти в хтмл var OMNI_API_URL = "https://d2xyrw75c4fk9m.cloudfront.net/graphql";


namespace Parser
{
    internal class Ecoatm
    {
        public dynamic Response { get; set; }
        public int StatusCode { get; set; }

        public Ecoatm(string imei, FormatProxy proxy = null)
        {
           Loger.Info("\nEcoatm...");

           var cookieContainer = new CookieContainer();
            GetRequest html = new GetRequest("https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer");
            if (proxy.Proxy != null)
                html.Proxy = proxy.Proxy;

            html.Referer = "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer";
            html.Host = "www.ecoatm.com";
            html.Run(cookieContainer);
            StatusCode = html.StatusCode;
            if (html.Response == null) return;

            Uri myUri = new Uri(TextParse.SubString(html.Response, ":\"Payout\"}};</script><script src=\"", "\""));

            GetRequest mainjs = new GetRequest(myUri.OriginalString);
            if (proxy != null)
                mainjs.Proxy = proxy.Proxy;

            mainjs.Referer = "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer";
            mainjs.Host = myUri.Host;
            mainjs.Run(cookieContainer);
            StatusCode = html.StatusCode;
            if (html.Response == null) return;

            string googleKey = TextParse.SubString(mainjs.Response, "execute-api.us-east-1.amazonaws.com/prod/\",s=\"", "\",H=\"https:");
            string cloudUrl = TextParse.SubString(html.Response, "var OMNI_API_URL = \"", "\";");
            //cloudUrl = cloudUrl.Insert(cloudUrl.IndexOf("cloudfront.net") + 14, ":8081");
            cloudUrl += "/graphql";
            Uri cloudfront = new Uri(cloudUrl);

            if (string.IsNullOrEmpty(googleKey))
            {
                Loger.Error("googleKey not found!");
                return;
            }

            string captchaResponse;
            try
            {
                XevilReCaptcha2 xe = new XevilReCaptcha2((string)Program.setting.dynamic.XevilHost, googleKey, "https://www.ecoatm.com/a/devices/apple/iphone/4gb/other/offer");
                captchaResponse = xe.Get();
            }
            catch (Exception ex)
            {
                Loger.Error("Xevil : " + ex.GetType().FullName);
                return;
            }


            // получаем инфу
            PostRequest post = new PostRequest(cloudfront.OriginalString);
            if (proxy != null)
                post.Proxy = proxy.Proxy;

            post.Referer = "https://www.ecoatm.com/";
            post.Host = cloudfront.Host;
            post.ContentType = "application/json";
            post.Accept = "*/*";
            post.Headers.Add("sec-fetch-dest", "empty");
            post.Headers.Add("sec-fetch-mode", "cors");
            post.Headers.Add("sec-fetch-site", "cross-site");
            post.Headers.Add("sec-gpc", "1");
            post.Useragent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            post.Data = "{\"operationName\":\"verifyIMEINumber\",\"variables\":{\"input\":{\"imeiNumber\":\"" + imei
                + "\",\"deviceBrand\":\"Apple\",\"financeLock\":\"no\",\"recaptchaToken\":\"" + captchaResponse
                + "\"}},\"query\":\"mutation verifyIMEINumber($input: VerifyIMEINumberInput) {\\n  verifyIMEINumber(input: $input) {\\n    data\\n    __typename\\n  }\\n}\\n\"}";
            post.Run(cookieContainer);
            StatusCode = html.StatusCode;

            if (post.Response != null)
            {
                try
                {
                    Loger.Info("Log Response:\n" + post.Response);
                    JObject obj = JObject.Parse(post.Response);
                    Response = JObject.Parse(obj["data"]["verifyIMEINumber"]["data"].ToString());
                    //Loger.Info((string)Response);
                }
                catch (Exception ex)
                {
                    Loger.Error("Ecoatm JSON parse : " + ex.GetType().FullName);
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
