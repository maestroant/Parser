using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static Parser.Visible;

namespace Parser
{
    internal class Visible
    {
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
            if (js.Response == null) return;

            string authorization = TextParse.SubString(js.Response, "API_TOKEN:\"", "\",");
            if (string.IsNullOrEmpty(authorization))
            {
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

            if (post.Response != null)
            {
                try
                {   
                    Loger.Info("Log Response:\n" + post.Response);
                    Response = JObject.Parse(post.Response);
                    IMEI1 = Response.IMEI1;
                    IMEI2 = Response.IMEI2;
                    Loger.Info("IMEI1: " + IMEI1 + "\nIMEI2: " + IMEI2);
                }
                catch (Exception ex)
                {
                    Loger.Error("Visible JSON parse : " + ex.GetType().FullName);
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
