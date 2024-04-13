/*
 * This file is part of the Console_Selenium_Serilog_Template Project.
 * 
 * This library is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library. If not, see <https://www.gnu.org/licenses/>.
 * 
 * (c) 2020-2024 Braxton Bell
 * bell.braxton@outlook.com
 * https://www.linkedin.com/in/braxton-bell/
 */


using Console_Selenium_Serilog_Template.Config;
using Console_Selenium_Serilog_Template.Utilities;
using Console_Selenium_Serilog_Template.Webkit.Profiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace Console_Selenium_Serilog_Template.Webkit;

public enum WebDriverType
{
    Local = 0,
    Remote = 1
}

public class WrapWebDriver : IDisposable, IWrapWebDriver
{
    private readonly ILogger<WrapWebDriver> _logger;
    private readonly ILogPropertyMgr _propMgr;
    private readonly IOptions<ApplicationConfig> _options;
    private readonly IProfileManager _profileMgr;
    private readonly string _browserSessionId;
    private readonly WebDriverType _browserProfileType;
    private readonly bool _isPersistentProfileEnabled;
    private int _driverFailCount = 0; // Used for backoff logic
    private bool _disposedValue = false;
    private IWebDriver _webDriver;
    private bool _isWebDriverStarted = false;
    private int _lastLogIndex = 0; // Used to track the last log index of Chrome browser logs

    public string BrowserSessionId => _browserSessionId;

    public IWebDriver WebDriver
    {
        get
        {
            if (!_isWebDriverStarted) throw new Exception("WebDriver is not started.");
            return _webDriver;
        }
        private set => _webDriver = value;
    }

    private IOptions<ApplicationConfig> Options => _options;

    public WrapWebDriver(
        ILogger<WrapWebDriver> logger,
        ILogPropertyMgr propMgr,
        IOptions<ApplicationConfig> options,
        IProfileManager profileMgr)
    {
        _browserSessionId = Guid.NewGuid().ToString();
        _logger = logger;
        _propMgr = propMgr;
        _options = options;
        _profileMgr = profileMgr;

        // Add browser session id to the logger properties
        _propMgr.Add("BrowserSessionId", _browserSessionId);

        _browserProfileType = _options.Value.BrowserConfig.BrowserProfile;
        _isPersistentProfileEnabled = _options.Value.BrowserConfig.EnablePersistentProfile;
    }

    /// <summary>
    /// Starts logging Chrome browser logs to the logger.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartLoggingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var logs = _webDriver.Manage().Logs.GetLog(LogType.Browser);

                for (int i = _lastLogIndex; i < logs.Count; i++)
                {
                    var entry = logs[i];
                    // Log the message
                    _propMgr.Add("ChromeLog", entry.Message);
                    using (_logger.BeginScope(_propMgr.GetProperties()))
                    {
                        _logger.LogInformation("Chrome Debug Message: {Log_Level}", entry.Level);
                    }
                    _propMgr.Drop("ChromeLog");
                }

                _lastLogIndex = logs.Count;

                // Wait asynchronously for a short interval
                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // The delay was canceled, exit the loop
                    break;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _logger.LogWarning(ex, "Error in {Method}", nameof(StartLoggingAsync));
            }
        }
    }

    public IOperationResult StartDriver()
    {
        IOperationResult result = OperationResult.Ok();

        if (_isWebDriverStarted && CheckIsHealthyDriver())
        {
            _logger.LogInformation("{Method}: WebDriver is already started.", nameof(StartDriver));
        }
        else if (_isWebDriverStarted && !CheckIsHealthyDriver())
        {
            _logger.LogWarning("{Method}: WebDriver is already started but not healthy.", nameof(StartDriver));
            result = OperationResult.Fail();
        }
        else
        {
            result = Initialize();
        }

        return result;
    }

    public IOperationResult RestartDriver(double delay = 5)
    {
        IOperationResult result = OperationResult.Ok();

        StopDriver();

        Thread.Sleep(TimeSpan.FromSeconds(delay));

        result = StartDriver();

        return result;
    }

    public IOperationResult StopDriver()
    {
        var result = OperationResult.Ok();

        if (_webDriver != null)
        {
            try
            {
                _webDriver.Quit();
                _webDriver = null;
                _isWebDriverStarted = false;
                _lastLogIndex = 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Method}: Error stopping WebDriver.", nameof(StopDriver));
                result = OperationResult.Fail();
            }
        }

        return result;
    }

    public bool CheckIsHealthyDriver()
    {
        var isHealthy = false;

        if (ExistWebDriverException())
        {
            _logger.LogWarning("{Method}: WebDriver is not healthy.", nameof(CheckIsHealthyDriver));
        }
        else
        {
            _logger.LogDebug("{Method}: WebDriver is healthy.", nameof(CheckIsHealthyDriver));
            isHealthy = true;
        }

        return isHealthy;
    }

    // ---------------------------------- Begin Initialize WebDriver ----------------------------------
    private IOperationResult Initialize()
    {
        using (_logger.BeginScope(_propMgr.GetProperties()))
        {
            _logger.LogDebug("{Method}: Initializing WebDriver.", nameof(Initialize));

            var result = OperationResult.Ok();
            var chromeOpt = GetChromeOptions();
            if (chromeOpt == null)
            {
                _logger.LogWarning("{Method}: ChromeOptions is null.", nameof(Initialize));
                return OperationResult.Fail();
            }

            var sAppDirectory = AppContext.BaseDirectory;
            var driverType = GetDriverType(ref result);
            if (!result.Success) return result;

            result = InitializeWebDriver(driverType, sAppDirectory, chromeOpt);
            if (!result.Success) return result;

            result = VerifyDriverHealth();
            return result;
        }
    }

    private WebDriverType GetDriverType(ref OperationResult result)
    {
        try
        {
            return _options.Value.BrowserConfig.BrowserProfile;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method}: Error getting BrowserProfile from appsettings.json.", nameof(GetDriverType));
            result = OperationResult.Fail();
            return default;
        }
    }

    private OperationResult InitializeWebDriver(WebDriverType driverType, string sAppDirectory, ChromeOptions chromeOpt)
    {
        switch (driverType)
        {
            case WebDriverType.Local:
                return InitializeLocalWebDriver(sAppDirectory, chromeOpt);
            case WebDriverType.Remote:
                return InitializeRemoteWebDriver(chromeOpt);
            default:
                _logger.LogWarning("{Method}: Invalid BrowserProfile: {BrowserProfile}", nameof(InitializeWebDriver), driverType);
                return OperationResult.Fail();
        }
    }

    private OperationResult InitializeLocalWebDriver(string sAppDirectory, ChromeOptions chromeOpt)
    {
        try
        {
            // _webDriver = new ChromeDriver($"{sAppDirectory}\\browser\\", chromeOpt);
            _webDriver = new ChromeDriver(chromeOpt); // using DriverManager to automatically download and manage the driver
            _isWebDriverStarted = true;
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method}: Error in WebDriver initialization", nameof(InitializeLocalWebDriver));
            _driverFailCount++;
            DriverFailBackOff();
            StopDriver();
            return OperationResult.Fail();
        }
    }

    private OperationResult InitializeRemoteWebDriver(ChromeOptions chromeOpt)
    {
        var seleniumHub = GetSeleniumGridHub();
        if (seleniumHub == null)
        {
            _logger.LogWarning("{Method}: SeleniumGridHub is not defined in appsettings.json", nameof(InitializeRemoteWebDriver));
            return OperationResult.Fail();
        }

        try
        {
            _webDriver = new RemoteWebDriver(new Uri(seleniumHub), chromeOpt);
            _isWebDriverStarted = true;
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method}: Error connecting to Selenium Grid Hub.", nameof(InitializeRemoteWebDriver));
            _driverFailCount++;
            DriverFailBackOff();
            StopDriver();
            return OperationResult.Fail();
        }
    }

    private string GetSeleniumGridHub()
    {
        try
        {
            return _options.Value.BrowserConfig.SeleniumGridHub;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method}: Error getting SeleniumGridHub from appsettings.json.", nameof(GetSeleniumGridHub));
            return null;
        }
    }

    private OperationResult VerifyDriverHealth()
    {
        if (!CheckIsHealthyDriver())
        {
            _logger.LogWarning("{Method}: WebDriver is started but not healthy.", nameof(VerifyDriverHealth));
            return OperationResult.Fail();
        }

        _logger.LogDebug("{Method}: WebDriver is started.", nameof(VerifyDriverHealth));
        return OperationResult.Ok();
    }

    private void DriverFailBackOff()
    {
        int backOffTime = 0;
        if (_driverFailCount > 0)
        {
            backOffTime = (int)Math.Pow(3, _driverFailCount);
        }
        _logger.LogWarning("{Method}: WebDriver failed {DriverFailCount} times. Backing off for {BackOffTime} seconds.",
                           nameof(DriverFailBackOff), _driverFailCount, backOffTime);

        // let other threads/processes use the CPU
        Thread.Sleep(TimeSpan.FromSeconds(backOffTime));
    }
    // ---------------------------------- End Initialize WebDriver ----------------------------------

    private bool ExistWebDriverException()
    {
        var existException = false;

        try
        {
            var webElement = WebDriver.FindElement(By.TagName("body"));
        }
        catch (Exception ex)
        {
            existException = true;
            _logger.LogWarning(ex, "{Method}: Exception in WebDriver.", nameof(ExistWebDriverException));
        }

        return existException;
    }

    private ChromeOptions GetChromeOptions()
    {
        var chromeOpt = new ChromeOptions();

        ChromiumPerformanceLoggingPreferences perfLogPrefs = null;
        Proxy proxy = null;

        try
        {
            // -------------- Browser preferences --------------
            //chromeOpt.AddArgument("--disable-dev-shm-usage");
            //chromeOpt.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            chromeOpt.AddArgument("--disable-blink-features=AutomationControlled");

            // -------------- Logging preferences --------------
            // chromeOpt.SetLoggingPreference(LogType.Browser, OpenQA.Selenium.LogLevel.Debug);
            chromeOpt.AddArgument("--log-level=1");

            // -------------- Anti-captcha extension --------------
            if (Options.Value.BrowserConfig.EnableAntiCaptchaExt)
            {
                chromeOpt.AddExtension($"{Directory.GetCurrentDirectory()}/browser/extensions/anticaptcha-plugin.zip");
            }

            // -------------- Add Proxy settings --------------
            proxy = new Proxy();
            if (Options.Value.BrowserConfig.EnableProxy)
            {
                proxy.Kind = ProxyKind.Manual;
                proxy.IsAutoDetect = false;
                proxy.HttpProxy = _options.Value.BrowserConfig.ProxySettings.HttpProxy;
                proxy.SslProxy = _options.Value.BrowserConfig.ProxySettings.SslProxy;
                proxy.AddBypassAddress("127.0.0.1");
                chromeOpt.Proxy = proxy;

                _logger.LogDebug("{Method}: Proxy is enabled. HttpProxy: {HttpProxy} SslProxy: {SslProxy}", nameof(GetChromeOptions), proxy.HttpProxy, proxy.SslProxy);
            }
            else
            {
                _logger.LogDebug("{Method}: Proxy is not enabled.", nameof(GetChromeOptions));
            }
            // Alternative method: `chromeOpt.AddArgument($"--proxy-server={_sessionConfigMgr.HttpProxy}")`

            // -------------- Add User-Profile settings --------------
            ChromeOptions profileOptions = null;
            if (_isPersistentProfileEnabled)
            {
                profileOptions = _profileMgr.BuildChromeOptions();
            }

            // -------------- Merge ChromeOption (arguments) --------------
            if (profileOptions != null)
            {
                foreach (var arg in profileOptions.Arguments)
                {
                    chromeOpt.AddArgument(arg);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in {Method}", nameof(GetChromeOptions));
        }

        return chromeOpt;
    }

    /// <summary>
    /// Navigates to the specified URL and waits for the page to load.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="timeout">The timeout in seconds for the page to load.</param>
    /// <param name="matchUrl">Check if the current URL matches the specified URL either exactly or partially.</param>
    /// <param name="recursiveUrl">Allow partial match of URL.</param>
    /// <param name="requirePageLoad">Require the page to load before returning.</param>
    /// <param name="forceReload">True to force a page load by navigating to about:blank first.
    /// Useful for pages that do not update when navigating to the same URL.</param>
    public void NavigatePage(string url,
                             double timeout = 10,
                             bool matchUrl = true,
                             bool recursiveUrl = true,
                             bool requirePageLoad = true,
                             bool forceReload = false)
    {
        // Catch empty Url parameter
        if (string.IsNullOrEmpty(url)) throw new Exception("Url parameter cannot be empty");

        var isPageLoadComplete = false;
        var isUrlVerified = false;

        if (forceReload)
        {
            WebDriver.Navigate().GoToUrl("about:blank");
        }

        WebDriver.Navigate().GoToUrl(url);

        if (requirePageLoad)
        {
            var driverWait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeout));
            isPageLoadComplete = driverWait.Until(x =>
            {
                try
                {
                    // .Equals() returns a Boolean. WebDriverWait will cycle this until True or timer.
                    return ((IJavaScriptExecutor)WebDriver).ExecuteScript("return document.readyState").Equals("complete");
                }
                catch (Exception ex)
                {
                    // Call handler and return false on any error.
                    _logger.LogWarning(ex, "{Method}: An error occurred while waiting for page load.", nameof(NavigatePage));
                    return false;
                }
            });

            // Throw an exception if the page load or URL verification failed
            if (!isPageLoadComplete)
            {
                throw new Exception($"Error loading page. [{url}]");
            }
        }

        // Logic to verify Url, waits for Url to match or timeout
        if (matchUrl)
        {
            var urlString = "";
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            do
            {
                try
                {
                    urlString = WebDriver.Url;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{Method}: An error occurred while attempting to get URL.", nameof(NavigatePage));
                }

                if (urlString == url)
                {
                    isUrlVerified = true;
                    stopwatch.Stop();
                    break;
                }
                else if (recursiveUrl && urlString.Contains(url))
                {
                    isUrlVerified = true;
                    stopwatch.Stop();
                    break;
                }

                // Delay in the loop to allow other threads/processes to use the CPU
                Thread.Sleep(200);
            } while (stopwatch.ElapsedMilliseconds <= timeout * 1000);
            stopwatch.Stop();

            // Throw an exception if the page load or URL verification failed
            if (!isUrlVerified)
            {
                throw new Exception($"Error verifying URL. [{url}]");
            }
        }
    }

    public void ClickElement(By driverBy, int timeout, bool requirePageLoad = true)
    {
        if (requirePageLoad)
        {
            // Wait for the element to be present in the DOM
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromMilliseconds(timeout));
            var element = wait.Until(d => d.FindElement(driverBy));
            element.Click();

            wait.Until(d =>
            {
                try
                {
                    return ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{Method}: An error occurred while attempting to click element.", nameof(ClickElement));
                    return false;
                }
            });
        }
        else
        {
            WebDriver.FindElement(driverBy).Click();
        }
    }

    public bool IsPageReady(string uri, double timeout = 10, bool recursiveUri = true)
    {
        if (uri == null) throw new Exception("Null is not a valid parameter.");

        // Initialize a tracker to track exceptions
        var tracker = ExceptionTracker.Create();

        var passUrlChecks = false;
        string currentUrl = null;

        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        do
        {
            // Catch any error thrown by _chromeDriver.URL and assign empty string value to sDriver_URL
            try
            {
                currentUrl = WebDriver.Url;
            }
            catch (Exception ex)
            {
                // Add the exception to the tracker
                tracker.TrackException(ex);
                currentUrl = "";
            }

            if (!recursiveUri && currentUrl == uri) // Check if the current URL matches the specified URL exactly
            {
                passUrlChecks = true;
                timer.Stop();
                break;
            }
            else if (recursiveUri && currentUrl.Contains(uri)) // Check if the current URL contains the specified URL
            {
                passUrlChecks = true;
                timer.Stop();
                break;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        // Log aggregate exception from the inner try block
        _logger.LogExceptionCounts(tracker, $"{nameof(IsPageReady)}");

        if (!passUrlChecks)
        {
            // Log urls for debugging on separate lines
            _logger.LogDebug("{Method}: Page not ready. Current URL: {CurrentUrl} Specified URL: {SpecifiedUrl}",
                             nameof(IsPageReady), currentUrl, uri);
        }

        return passUrlChecks;
    }

    /// <summary>
    /// Checks if the specified page is already loaded, and if not, attempts to navigate to it.
    /// </summary>
    /// <param name="uriParam">The URI of the page to check or navigate to.</param>
    /// <param name="timeout">The timeout in seconds for the page to load.</param>
    /// <param name="recursiveUri">Allow partial match of URL.</param>
    /// <param name="forceReload">True to force a page load by navigating to about:blank first. 
    /// Useful for pages that do not update when navigating to the same URL.</param>
    /// <returns>True if the page is ready (either already loaded or successfully navigated to); False otherwise.</returns>
    public bool TryIsPageReady(string uriParam, double timeout = 10, bool recursiveUri = true, bool forceReload = false)
    {
        var pageReady = IsPageReady(uriParam, timeout, recursiveUri);

        if (!pageReady)
        {
            try
            {
                NavigatePage(uriParam, timeout, true, recursiveUri, true, forceReload);
                pageReady = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method}: Error verifying page.", nameof(TryIsPageReady));
                pageReady = false;
            }
        }

        return pageReady;
    }

    public IReadOnlyCollection<IWebElement> GetElements(By driveryBy)
    {
        IReadOnlyCollection<IWebElement> webElements = null;

        try
        {
            webElements = WebDriver.FindElements(driveryBy);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method}: An error occurred while getting elements.", nameof(GetElements));
        }

        if (webElements == null || webElements.Count < 1)
        {
            throw new Exception($"Error getting [{driveryBy}]");
        }

        return webElements;
    }

    public IWebElement GetElement(By driveryBy, double timeoutSeconds = 10)
    {
        IWebElement webElement = null;
        WebDriverWait webDriverWait = null;

        // Initialize a tracker to track exceptions
        var tracker = ExceptionTracker.Create();

        try
        {
            webDriverWait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(timeoutSeconds));
            webElement = webDriverWait.Until(d =>
            {
                try
                {
                    return WebDriver.FindElement(driveryBy);
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    // Add the exception to the tracker and return null
                    tracker.TrackException(ex);
                    return null;
                }
            });
        }
        catch (Exception ex)
        {
            // Log aggregate exception from the inner try block
            _logger.LogExceptionCounts(tracker, nameof(GetElement));

            // Rethrow the exception
            throw;
        }
        finally
        {
            webDriverWait = null;
        }

        return webElement;
    }

    public async Task<bool> AssertIsDisplayedAsync(By driverBy, double timeout = 0)
    {
        var status = false;
        var isDisplayed = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                var element = WebDriver.FindElement(driverBy);
                isDisplayed = element.Displayed;
                if (isDisplayed)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not displayed
                status = false;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            await Task.Delay(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsDisplayed(By driverBy, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                var element = WebDriver.FindElement(driverBy);
                isDisplayed = element.Displayed;
                if (isDisplayed)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not displayed
                status = false;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsDisplayed(IWebElement element, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                isDisplayed = element.Displayed;
                if (isDisplayed)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not displayed
                status = false;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsNotDisplayed(By driverBy, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                var element = WebDriver.FindElement(driverBy);
                isDisplayed = element.Displayed;
                if (!isDisplayed)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not displayed
                status = true;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsNotDisplayed(IWebElement element, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                isDisplayed = element.Displayed;
                if (!isDisplayed)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not displayed
                status = true;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsClickable(By driverBy, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;
        var isEnabled = false;

        var timer = new Stopwatch();
        timer.Start();
        do
        {
            try
            {
                var element = WebDriver.FindElement(driverBy);
                isDisplayed = element.Displayed;
                isEnabled = element.Enabled;
                if (isDisplayed && isEnabled)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not clickable
                status = false;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsClickable(IWebElement element, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;
        var isEnabled = false;

        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        do
        {
            try
            {
                isDisplayed = element.Displayed;
                isEnabled = element.Enabled;
                if (isDisplayed && isEnabled)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not clickable
                status = false;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsNotClickable(By driverBy, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;
        var isEnabled = false;

        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        do
        {
            try
            {
                var element = WebDriver.FindElement(driverBy);
                isDisplayed = element.Displayed;
                isEnabled = element.Enabled;
                if (!isDisplayed && !isEnabled)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not clickable
                status = true;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    public bool AssertIsNotClickable(IWebElement element, double timeout = 10)
    {
        var status = false;
        var isDisplayed = false;
        var isEnabled = false;

        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        do
        {
            try
            {
                isDisplayed = element.Displayed;
                isEnabled = element.Enabled;
                if (!isDisplayed && !isEnabled)
                {
                    status = true;
                    timer.Stop();
                    break;
                }
            }
            catch (Exception)
            {
                // On error assume that the element is not clickable
                status = true;
            }

            // Delay in the loop to allow other threads/processes to use the CPU
            Thread.Sleep(200);
        } while (timer.ElapsedMilliseconds <= timeout * 1000);
        timer.Stop();

        return status;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)

                // Close the ProfileManager if persistent profile is enabled
                //if (_isPersistentProfileEnabled &&
                //    _profileMgr != null &&
                //    _profileMgr.IsProfileLoaded &&
                //    _webDriver != null)
                //{
                //    _logger.LogDebug("{Method}: Closing ProfileManager and archiving profile.", nameof(Dispose));

                //    _profileMgr.CloseStatefullSession();
                //}

                StopDriver();
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null

            _disposedValue = true;
        }
    }

    // Override finalizer only if Dispose(bool disposing) has code to free unmanaged resources
    // ~WrapWebDriver()
    // {
    //     // Do not change this code. Put cleanup code in Dispose(bool disposing) method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}