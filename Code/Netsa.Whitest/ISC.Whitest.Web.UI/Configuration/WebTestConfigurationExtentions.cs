﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISC.Whitest.Web.Core;
using ISC.Whitest.Web.UI.PageObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;

namespace ISC.Whitest.Web.UI.Configuration
{
    public static class WebTestConfigurationExtentions
    {
        public static WebTestConfiguration UseChrome(this WebTestConfiguration configuration, string driverPath)
        {
            return Config(configuration, ()=> new ChromeDriver(driverPath));
        }
        public static WebTestConfiguration UseChrome(this WebTestConfiguration configuration)
        {
            return Config(configuration, () => new ChromeDriver());
        }
        public static WebTestConfiguration UsePhantomJs(this WebTestConfiguration configuration, string driverPath)
        {
            return Config(configuration, ()=> new PhantomJSDriver(driverPath));
        }
        public static WebTestConfiguration UsePhantomJs(this WebTestConfiguration configuration)
        {
            return Config(configuration, () => new PhantomJSDriver());
        }
        private static WebTestConfiguration Config(WebTestConfiguration configuration
            ,Func<IWebDriver> driverFunc)
        {
            WebUITestConfiguration.BaseUrl = configuration.BaseUrl;
            WebUITestConfiguration.DriverFactory = driverFunc;

            return configuration;
        }
    }
}
