using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class Setting
    {

        public readonly dynamic dynamic;

        public Setting(string nameFile)
        {
            try
            {
                JObject obj = JObject.Parse(File.ReadAllText(nameFile));
                dynamic = obj;
            }
            catch (Exception ex)
            {
                Loger.Error("Setting : " + ex.GetType().FullName);
                Process.GetCurrentProcess().Kill();
            }

        }

    }
}
