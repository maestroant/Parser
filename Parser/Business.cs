using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    internal class Business
    {
        public dynamic Response { get; set; }
        public bool BlackListed { get; set; }
        public int StatusCode { get; set; }

        public Business(string imei, FormatProxy proxy = null)
        {
            Loger.Info("\nBusiness...");
            var cookieContainer = new CookieContainer();
            PostRequest post = new PostRequest("https://www.business.att.com/restservices/midmarket/v1/byod/verifyimei");
            if (proxy.Proxy != null)
                post.Proxy = proxy.Proxy;
            post.Host = "www.business.att.com";
            post.ContentType = "application/json";
            post.Accept = "application/json";
            post.Data = "{\"imei\": \"" + imei + "\"}\n"; 
            post.Run(cookieContainer);
            StatusCode = post.StatusCode;

            if (post.Response != null)
            {
                try
                {
                    Loger.Info("Log Response:\n" + post.Response);
                    Response = JObject.Parse(post.Response);
                    BlackListed = Response.body.blackListed;
                    Loger.Info("BlackListed = " + BlackListed);
                }
                catch (Exception ex)
                {
                    Loger.Error("Business JSON parse : " + ex.GetType().FullName);
                }
            }
        }
    }
}


//{
//   "headers":{

//   },
//   "body":{
//      "imei":"352844115151198",
//      "imeiType":"S6",
//      "deviceType":"phone",
//      "attCompatibility":true,
//      "blackListed":false,
//      "deviceRecognized":true,
//      "imeiCarrierLocked":false,
//      "hasEsim":false,
//      "device":{
//         "description":"iPhone LTE",
//         "url":"https://www.wireless.att.com/business/images/equip/smartphone_default.gif",
//         "parentItemId":"prod11796700043",
//         "imeiTypeCategory":"Integrated-Smartphone",
//         "itemId":"sku11819600046",
//         "modelName":"DUMMY J0",
//         "compatibleSimTypes":"physicalSim",
//         "defaultDeviceSimType":"physicalSim",
//         "deviceImeiTypeId":"S6",
//         "compatibleRatePlan":[
//            "sku12007900018",
//            "sku12007900017",
//            "sku12007900016",
//            "sku12007900015",
//            "sku12007900019",
//            "sku12016600009",
//            "sku12007900014",
//            "sku12041500017",
//            "sku12041500016",
//            "sku12041500015",
//            "sku12041500014",
//            "sku12016000005",
//            "sku12016000006",
//            "sku12007900029",
//            "sku12007900028",
//            "sku12007900027",
//            "sku12007900026",
//            "sku12007900021",
//            "sku12007900020",
//            "sku12007900025",
//            "sku12007900024",
//            "sku12007900023",
//            "sku12007900001",
//            "sku12007900022"
//         ]
//    },
//      "geoTab":false,
//      "esimCardCapable":false,
//      "dscapable":false,
//      "esimCapable":false
//   },
//   "statusCode":"ACCEPTED",
//   "statusCodeValue":202
//}
