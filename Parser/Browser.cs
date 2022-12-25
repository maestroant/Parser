using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;   // https://github.com/RDavydenko/OpenQA.Selenium.Chrome.ChromeDriverExtensions
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

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
            options.AddArgument("--blink-settings=imagesEnabled=false");
            options.AddArgument("--dns-prefetch-disable");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--no-default-browser-check");
            //options.AddArguments("headless");

            try
            {
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                ChromeDriver driver = new ChromeDriver(driverService, options);
                driver.Navigate().GoToUrl(url);
                OpenQA.Selenium.Cookie cookie = null;
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                cookie = wait.Until(drv => drv.Manage().Cookies.GetCookieNamed(nameCookie));

                driver.Close();
                driver.Quit();
                if (cookie != null)
                    return cookie.Value;
                else
                    return null;

            }
            catch (Exception ex)
            {
                Loger.Error(ex, "Browser");
                return null;
            }
        }
    }
}
