#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.Network;

namespace Selenium_4_dotnet
{
    public class Tests
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
        /// Network Emulation as Offline with Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task EmulateOfflineNetwork()
        {
            await _session.Network.Enable(new EnableCommandSettings()
            {
                MaxTotalBufferSize = 100000000
            });
            
            await _session.Network.EmulateNetworkConditions(new EmulateNetworkConditionsCommandSettings()
            {
                Offline = true,
                Latency = 100,
                DownloadThroughput = 1000,
                UploadThroughput = 2000,
                ConnectionType = ConnectionType.Cellular3g
            });
            var loadingFailedSync = new ManualResetEventSlim(false);

            void NetworkEmulationHandler(object? sender, LoadingFailedEventArgs e)
            {
                Assert.That(e.ErrorText, Is.EqualTo("net::ERR_INTERNET_DISCONNECTED"));
                loadingFailedSync?.Set();
            }

            _session.Network.LoadingFailed += NetworkEmulationHandler;

            _driver.Url = "http://thoughtworks.com";
            //Added just for users to see what happens on the browser during the execution - remove it!
            Thread.Sleep(10000);
            loadingFailedSync.Wait(TimeSpan.FromSeconds(5));
        }
        
        /// <summary>
        /// Network Emulation as Online with Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task EmulateOnlineNetwork()
        {
            await _session.Network.Enable(new EnableCommandSettings()
            {
                MaxTotalBufferSize = 100000000
            });
            await _session.Network.EmulateNetworkConditions(new EmulateNetworkConditionsCommandSettings()
            {
                Offline = false,
                Latency = 100,
                DownloadThroughput = 1000,
                UploadThroughput = 2000,
                ConnectionType = ConnectionType.Cellular4g
            });
            var loadSync = new ManualResetEventSlim(false);

            _driver.Url = "http://thoughtworks.com";
            
            //Added just for users to see what happens on the browser during the execution - remove it!
            Thread.Sleep(10000);
            loadSync.Wait(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Network Interceptor with Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task TestInterceptNetwork()
        {
            await _session.Network.Enable(new EnableCommandSettings());
            await _session.Network.SetBlockedURLs(new SetBlockedURLsCommandSettings()
            {
                Urls = new string[] { "*://*/*.css" }
            });
            
            ManualResetEventSlim loadSync = new ManualResetEventSlim(false);
            EventHandler<LoadingFailedEventArgs> loadingFailedHandler = (sender, e) =>
            {
                if (e.Type == ResourceType.Stylesheet)
                {
                    Console.WriteLine(e.ErrorText);
                    Assert.That(e.BlockedReason == BlockedReason.Inspector);
                }

                loadSync.Set();
            };
            _session.Network.LoadingFailed += loadingFailedHandler;

            _driver.Url = "http://twitter.com";
            
            //Added just for users to see what happens on the browser during the execution - remove it!
            Thread.Sleep(10000);
            loadSync.Wait(TimeSpan.FromSeconds(5));
        }
        /// <summary>
        /// By pass Service worker with Selenium 4 .NET Core
        /// </summary>
        [Test]
        public async Task TestBypassServiceWorker()
        {
            await _session.Network.Enable(new EnableCommandSettings());
            await _session.Network.SetBypassServiceWorker(new SetBypassServiceWorkerCommandSettings()
            {
                Bypass = true,
            });

        }
        [TearDown]
        public void TearDown()
        {
            if (_session != null) _session.Dispose();
            if (_driver != null) _driver.Close();
        }
    }
}