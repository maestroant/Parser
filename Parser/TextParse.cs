using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal static class TextParse
    {
        // text - текст, label - метка по которой ищем нужное местоб
        // strtStr - место по которому ищем начиная после label, endStr - строка которая идет сразу за нужной нам подстрокой
        // возвращает подстроку которая находится между strtStr и endStr, если не нашел - null
        public static string SubString(string text, string label, string strtStr, string endStr)
        {
            int strtIndex = text.IndexOf(label);
            if (strtIndex < 0) return null;
            strtIndex += label.Length;
            if ((strtIndex = text.IndexOf(strtStr, strtIndex)) < 0) return null;
            strtIndex += strtStr.Length;

            int endIndex = text.IndexOf(endStr, strtIndex);
            if (endIndex < 0) return null;

            return text.Substring(strtIndex, endIndex - strtIndex);
        }

        // возвращает подстроку которая находится между strtStr и endStr, если не нашел - null
        public static string SubString(string text, string strtStr, string endStr)
        {
            int strtIndex = text.IndexOf(strtStr);
            if (strtIndex < 0) return null;
            strtIndex += strtStr.Length;

            int endIndex = text.IndexOf(endStr, strtIndex);
            if (endIndex < 0) return null;

            return text.Substring(strtIndex, endIndex - strtIndex);
        }


        // возвращает подстроку которая находится между strtStr и концом строки
        public static string SubString(string text, string strtStr)
        {
            int strtIndex = text.IndexOf(strtStr);
            if (strtIndex < 0) return null;
            strtIndex += strtStr.Length;

            int endIndex = text.Length;
            if (endIndex < 0) return null;

            return text.Substring(strtIndex, endIndex - strtIndex);
        }
    }
}
