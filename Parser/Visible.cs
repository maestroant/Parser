using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Parser
{
    internal class Visible
    {
        public enum ResultEnum
        {
            OK,
            ERROR_HTTP,    // ошибкаб авторизации, таймат, что угодно
            ERROR_SOCKS,   // ошибка 503 
            ERROR_RESULT,  // ошибка не верный формат отдачи
            WRONG_RESULT   // если модели нет или не та что нужно
        }

        public ResultEnum Result { get; set; }

        public dynamic Response { get; set; }
        public string IMEI1 { get; set; }
        public string IMEI2 { get; set; }
        public int StatusCode { get; set; }

        public Visible(string imei, FormatProxy proxy = null)
        {
            Loger.Info("\nVisible...");

            var cookieContainer = new CookieContainer();
            GetRequest js = new GetRequest("https://www.visible.com/shop/js/59.9637ae7a647e87cd881b.chunk.js");
            if (proxy.Proxy != null)
                js.Proxy = proxy.Proxy;
            js.Host = "www.visible.com";
            js.Run(cookieContainer);
            StatusCode = js.StatusCode;

            if (string.IsNullOrEmpty(js.Response))
            {
                if (StatusCode == 407)
                {
                    Result = ResultEnum.ERROR_SOCKS;
                    return;
                }
                Result = ResultEnum.ERROR_HTTP;
                return;
            }

            string authorization = TextParse.SubString(js.Response, "API_TOKEN:\"", "\",");
            if (string.IsNullOrEmpty(authorization))
            {

                Result = ResultEnum.ERROR_HTTP;
                Loger.Error("API_TOKEN not found!");
                return;
            }

            PostRequest post = new PostRequest("https://api.bevisible.com/v1/nativeapp/compatibilityDetails?network=core");
            if (proxy != null)
                post.Proxy = proxy.Proxy;
            post.Host = "api.bevisible.com";
            post.ContentType = "application/json";
            post.Accept = "application/json";
            post.Headers.Add("Authorization", "Bearer " + authorization);
            post.Data = "{\"deviceId\": \"" + imei + "\"}\n";
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

            if (post.Response != null)
            {
                try
                {
                    Loger.Info("Log Response:\n" + post.Response);
                    Response = JObject.Parse(post.Response);
                    IMEI1 = Response.IMEI1;
                    IMEI2 = Response.IMEI2;
                    Loger.Info("IMEI1: " + IMEI1 + "\nIMEI2: " + IMEI2);
                    if (IMEI1 == null) Result = ResultEnum.ERROR_RESULT;
                    return;
                }
                catch (Exception ex)
                {
                    Loger.Error(ex, "Visible JSON parse");
                }
            }
        }
    }
}


//{
//            "deviceCompatible": "false",
//            "IMEI1":"352844115151198",
//            "IMEI2": "352844115232527",
//            "EID": "",
//            "preferredSoftSim": "",
//            "deviceLostStolenCompatibility": "",
//            "deviceLockCompatibility": "",
//            "compatibility" : "Y",
//            "swappable":"false",
//            "message":"",
//            "mfgCode":"APL",
//            "esimOnlyDevice":"false"
//            }
