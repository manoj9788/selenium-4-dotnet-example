#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.Console;
using OpenQA.Selenium.DevTools.Network;

namespace Selenium_4_dotnet
{
    public class PerformanceAndConsoleTests
    {
        private IDevTools? _devTools;
        private DevToolsSession _session;
        private IWebDriver? _driver;

        
        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _devTools = _driver as IDevTools;
            if (_devTools != null) _session = _devTools.CreateDevToolsSession();
        }

        /// <summary>
        /// Get Performance Logs from Chrome Devtools Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task GetPerformanceLogs()
        {
            await _session.Performance.Enable();
            _driver!.Url = "https://twitter.com";
            await _session.Performance.Disable();
        }
        
        /// <summary>
        /// Send and verify Console message Chrome Devtools Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task VerifyConsoleMessage()
        {
            const string consoleMessage = "Hello! This is sample Text sent to Console by Selenium and Devtools";

            
            ManualResetEventSlim sync = new ManualResetEventSlim(false);
            EventHandler<MessageAddedEventArgs> msgHandler = (sender, e) =>
            {
                Assert.That(e.Message.Text, Is.EqualTo(consoleMessage));
                sync.Set();
            };
            _session.Console.MessageAdded += msgHandler;

            await _session.Console.Enable();

            _driver.Url = "http://twitter.com";
            
            ((IJavaScriptExecutor)_driver).ExecuteScript("console.log('" + consoleMessage + "');");
            sync.Wait(TimeSpan.FromSeconds(5));
            _session.Console.MessageAdded -= msgHandler;

            await _session.Console.Disable();
        }
        
        [TearDown]
        public void TearDown()
        {
            _session?.Dispose();
            _driver?.Close();
        }
    }
}