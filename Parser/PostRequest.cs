using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Parser
{
    public class PostRequest
    {
        HttpWebRequest _request;
        string _address;

        public Dictionary<string, string> Headers { get; set; }

        public string Response { get; set; }
        public string Accept { get; set; }
        public string Host { get; set; }
        public string Data { get; set; }
        public string ContentType { get; set; }
        public WebProxy Proxy { get; set; }
        public string Referer { get; set; }
        public string Useragent { get; set; }
        public int StatusCode { get; set; }

        public PostRequest(string address)
        {
            _address = address;
            Headers = new Dictionary<string, string>();
        }

        public void Run(CookieContainer cookieContainer)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            try
            {
                _request = (HttpWebRequest)WebRequest.Create(_address);
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                _request.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                _request.Method = "POST";
                _request.CookieContainer = cookieContainer;
                _request.Proxy = Proxy;
                _request.Accept = Accept;
                _request.Host = Host;
                _request.ContentType = ContentType;
                _request.Referer = Referer;
                _request.UserAgent = Useragent;
                _request.AllowAutoRedirect = true;
                //_request.ContentType = "application/x-www-form-urlencoded";
                _request.KeepAlive = true;
                _request.Timeout = 60000;

                foreach (var pair in Headers)
                {
                    _request.Headers.Add(pair.Key, pair.Value);
                }

                var type = _request.GetType();
                var currentMethod = type.GetProperty("CurrentMethod", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_request);

                var methodType = currentMethod.GetType();
                methodType.GetField("ContentBodyNotAllowed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(currentMethod, false);

                using (var streamWriter = new StreamWriter(_request.GetRequestStream()))
                {
                    streamWriter.Write(Data);
                }

                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                StatusCode = (int)response.StatusCode;
                var stream = response.GetResponseStream();
                if (stream != null) Response = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("(407)") > -1) StatusCode = 407;
                Loger.Error(ex, _address);
            }
        }
    }
}
