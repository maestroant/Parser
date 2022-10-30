﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;   // https://github.com/RDavydenko/OpenQA.Selenium.Chrome.ChromeDriverExtensions
using System.Net;
using System.Threading;
using Cookie = OpenQA.Selenium.Cookie;

namespace Parser
{
    internal class Browser
    {
        public static string GetCookie(string url, string nameCookie, FormatProxy formatProxy)
        { 
            var options = new ChromeOptions();

            if (formatProxy.Proxy != null)
            {
                if (formatProxy.UserName != null)
                {
                    options.AddHttpProxy(formatProxy.Host, formatProxy.Port, formatProxy.UserName, formatProxy.Password);
                }
                else
                    options.AddArguments("--proxy-server=http://" + formatProxy.Host + ":" + formatProxy.Port.ToString());
            }

            options.AddArgument("ignore-certificate-errors");
            options.AddArgument("no-sandbox");

            try
            {
                ChromeDriver driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl(url);
                OpenQA.Selenium.Cookie cookie = null;
                for (var i = 0; i < 50; i++)
                {
                    if ((cookie = driver.Manage().Cookies.GetCookieNamed(nameCookie)) != null) break; // "a_token"
                    Thread.Sleep(100);
                }
            
                driver.Quit();
                if (cookie != null)
                    return cookie.Value;
                else
                    return null;

            }
            catch (Exception ex)
            {
                Loger.Error("Browser : " + ex.GetType().FullName);
                return null;
            }
        }
    }
}