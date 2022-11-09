using Newtonsoft.Json.Linq;
using System;
using System.IO;

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
                Loger.StopProgram();
            }

        }

    }
}
