using System;

namespace Parser
{

    internal class RandomProxy
    {
        public int indexProxy = 0;

        public FormatProxy Next()
        {
            string proxy;
            Random rnd = new Random();
            FormatProxy fProxy;
            lock (Program.ProxyList)
            {

                while (Program.ProxyList.Count > 0)
                {
                    indexProxy = rnd.Next(Program.ProxyList.Count);
                    if (indexProxy >= Program.ProxyList.Count) continue;

                    proxy = Program.ProxyList[indexProxy];
                    //Loger.Info("Proxy Set: " + proxy);
                    fProxy = new FormatProxy(proxy);
                    if (fProxy.Proxy == null)
                    {
                        DeleteBadProxy();
                        continue;
                    }

                    return fProxy;
                }

                Loger.Error("Proxy list Empty");
                Loger.StopProgram();
                return null;

            }
        }

        public void DeleteBadProxy()
        {
            lock (Program.ProxyList)
            {
                Loger.Info("Bad proxy: " + Program.ProxyList[indexProxy] + " deleted!");
                Program.ProxyList.RemoveAt(indexProxy);
                if (Program.ProxyList.Count < 1)
                {
                    Loger.Error("Proxy list Empty");
                    Loger.StopProgram();
                }
            }
        }

    }
}
