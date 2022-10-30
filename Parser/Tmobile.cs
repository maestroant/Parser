using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class Tmobile
    {
        public dynamic Response { get; set; }
        public string IMEI1 { get; set; }
        public string IMEI2 { get; set; }
        public int StatusCode { get; set; }

        public Tmobile(string imei, FormatProxy proxy = null)
        {
            Loger.Info("\nTmobile...");
            var cookieContainer = new CookieContainer();
            string authorization =  Browser.GetCookie("https://www.t-mobile.com/resources/bring-your-own-phone", "a_token", proxy);
            GetRequest get = new GetRequest("https://www.t-mobile.com/self-service-shop/v1/byod-check?imeiQuery=" + imei);

            if (proxy.Proxy != null)
                get.Proxy = proxy.Proxy;

            get.Referer = "https://www.t-mobile.com/resources/bring-your-own-phone";
            get.Host = "www.t-mobile.com";
            get.Headers.Add("Authorization", "Bearer " + authorization);
            get.Run(cookieContainer);
            StatusCode = get.StatusCode;

            if (get.Response != null)
            {
                try
                {
                    Loger.Info("Log Response:\n" + get.Response);
                    Response = JObject.Parse(get.Response);
                    IMEI1 = Response.deviceImei1;
                    IMEI2 = Response.deviceImei2;
                    Loger.Info("IMEI1: " + IMEI1 + "\nIMEI2: " + IMEI2);
                }
                catch (Exception ex)
                {
                    Loger.Error("Tmobile JSON parse : " + ex.GetType().FullName);
                }
            }

        }
    }
}


//{
//    "imei": "352844115151198",
//	"blockStatus":
//	{
//        "blockStatus": "UNBLOCKED"

//    },
//	"networkCompatibility":
//	{
//        "compatibility": "Fully Compatible",
//		"deviceManufacturer": "Apple",
//		"deviceMarketingName": "iPhone 11 Pro Max"

//    },
//	"serviceUp": true,
//	"eidCSN": "89049032005008882600054303192350",
//	"esimSupported": "Y",
//	"deviceImei1": "352844115151198",
//	"deviceImei2": "352844115232527",
//	"validEid": true,
//	"profileoverriderequired": false
//}