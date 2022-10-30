using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class FormatProxy
    {
        public WebProxy Proxy = null;
        public string Host = null;
        public int Port = 0;    
        public string UserName = null;
        public string Password = null;

        private static bool PingHost(string strIP, int intPort)
        {
            bool blProxy = false;
            try
            {
                TcpClient client = new TcpClient(strIP, intPort);

                blProxy = true;
            }
            catch (Exception ex)
            {
                Loger.Error("Error pinging proxy host:'" + strIP + ":" + intPort.ToString() + "'");
                return false;
            }
            return blProxy;
        }

        public FormatProxy(string urlProxy)
        {
            if (urlProxy == null)
            {
                Loger.Error("Error parse proxy: " + urlProxy);
                return;
            }

            int n = urlProxy.IndexOf(":");
            if (n < 0)
            {
                Loger.Error("Error parse proxy: " + urlProxy);
                return;
            }

            Host = urlProxy.Substring(0, n);

            n++;
            int c = urlProxy.IndexOf(":", n);

            // Если нет в строке логина:пароля
            if (c < 0)
            {
                this.Proxy = new WebProxy(urlProxy, true, null);
                Port = Int32.Parse(urlProxy.Substring(n, urlProxy.Length - n));
                return;
            }

            Port = Int32.Parse(urlProxy.Substring(n, c - n));

            string url = "http://" + urlProxy.Substring(0, c);
            n = c + 1;
            c = urlProxy.IndexOf(":", n);
            UserName =  urlProxy.Substring(n, c - n);
            c++;
            Password = urlProxy.Substring(c, urlProxy.Length - c);
            // Setup credentials
            ICredentials credentials = new NetworkCredential(UserName, Password);

            if (PingHost(Host, Port))
            {             
                // Setup proxy
                this.Proxy = new WebProxy(url, true);
                this.Proxy.Credentials = credentials;
            }

        }
    }
}
