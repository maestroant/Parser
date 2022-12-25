using System;

namespace Parser
{

    internal class RandomProxy
    {
        public int indexProxy = 0;
        Random rnd = new Random();

        public FormatProxy Next()
        {
            string proxy;
            
            FormatProxy fProxy;
            lock (Program.ProxyList)
            {

                while (Program.ProxyList.Count > 0)
                {

                    //if (Program.ProxyList.Count > 1)
                    //{
                    //    {
                    //        r = rnd.Next(Program.ProxyList.Count);
                    //    } while (r == indexProxy);

                    //    indexProxy = r;
                    //}
                    //else
                    //    indexProxy = 0;

                    int indexProxy = rnd.Next(Program.ProxyList.Count);
                    if (indexProxy >= Program.ProxyList.Count) continue;

                    proxy = Program.ProxyList[indexProxy];
                    
                    fProxy = new FormatProxy(proxy);
                    if (fProxy.Proxy == null)
                    {
                        DeleteBadProxy();
                        continue;
                    }

                    Loger.Info("Proxy Set: " + proxy);
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
