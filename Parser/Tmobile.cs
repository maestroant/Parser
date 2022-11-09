using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Parser
{
    internal class Tmobile
    {
        public enum ResultEnum
        {
            OK,
            ERROR_HTTP,    // ошибкаб авторизации, таймат, что угодно
            ERROR_SOCKS,   // ошибка 407 
            ERROR_RESULT,  // ошибка не верный формат отдачи
            WRONG_RESULT   // если модели нет или не та что нужно
        }

        public ResultEnum Result { get; set; }

        public dynamic Response { get; set; }
        public string IMEI1 { get; set; }
        public string IMEI2 { get; set; }
        public int StatusCode { get; set; }

        public Tmobile(string imei, FormatProxy proxy = null)
        {
            Loger.Info("\nTmobile...");
            var cookieContainer = new CookieContainer();
            string authorization = Browser.GetCookie("https://www.t-mobile.com/resources/bring-your-own-phone", "a_token", proxy);
            GetRequest get = new GetRequest("https://www.t-mobile.com/self-service-shop/v1/byod-check?imeiQuery=" + imei);

            if (proxy.Proxy != null)
                get.Proxy = proxy.Proxy;

            get.Referer = "https://www.t-mobile.com/resources/bring-your-own-phone";
            get.Host = "www.t-mobile.com";
            get.Headers.Add("Authorization", "Bearer " + authorization);
            get.Run(cookieContainer);
            StatusCode = get.StatusCode;

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

            if (get.Response != null)
            {
                try
                {
                    Loger.Info("Log Response:\n" + get.Response);
                    Response = JObject.Parse(get.Response);
                    IMEI1 = Response.deviceImei1;
                    IMEI2 = Response.deviceImei2;
                    Loger.Info("IMEI1: " + IMEI1 + "\nIMEI2: " + IMEI2);
                    string compatibility = Response.networkCompatibility.compatibility;
                    if (compatibility != null)
                        if (compatibility.IndexOf("NotFound", StringComparison.CurrentCultureIgnoreCase) > 0)
                        {
                            Result = ResultEnum.WRONG_RESULT;
                            return;
                        }

                    string deviceManufacturer = Response.networkCompatibility.deviceManufacturer;
                    if (deviceManufacturer != null)
                        if (deviceManufacturer.IndexOf("APPLE", StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            Result = ResultEnum.WRONG_RESULT;
                            return;
                        }

                    if ((compatibility == null) && (deviceManufacturer == null))
                    {
                        Result = ResultEnum.WRONG_RESULT;
                        return;
                    }


                    if (IMEI1 == null) Result = ResultEnum.ERROR_RESULT;
                    return;
                }
                catch (Exception ex)
                {
                    Loger.Error(ex, "Tmobile JSON");
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