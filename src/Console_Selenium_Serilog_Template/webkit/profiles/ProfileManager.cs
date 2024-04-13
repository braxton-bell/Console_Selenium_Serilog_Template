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
using Console_Selenium_Serilog_Template.Webkit.Profiles;
using Console_Selenium_Serilog_Template.Webkit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Console_Selenium_Serilog_Template.Utilities;

namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

/// <summary>
/// Manages browser profiles, handling creation, loading, and deletion as needed for browser instances.
/// </summary>
/// <remarks>
/// The <c>ProfileManager</c> class is responsible for managing the lifecycle of browser profiles used in web scraping or automated browsing sessions. Key functionalities include:
/// <list type="bullet">
/// <item>
/// <description>Profile Loading: Utilizes <c>IProfileLoader</c> to manage active browser profiles, ensuring that profiles are properly queued and loaded for use in browser sessions.</description>
/// </item>
/// <item>
/// <description>Chrome Options Building: Constructs ChromeOptions with the necessary parameters for the browser session, including user data directory and profile directory based on the loaded profile.</description>
/// </item>
/// <item>
/// <description>Profile Path Management: Retrieves and manipulates the file system paths related to browser profiles, facilitating operations like profile purge and path resolution.</description>
/// </item>
/// </list>
/// This class operates in conjunction with services like <c>IWrapWebDriver</c> for browser control and <c>ILogPropertyMgr</c> for logging contextual information.
/// Error handling is integrated throughout to ensure robustness and to log any issues during profile management operations.
/// </remarks>
public class ProfileManager : IProfileManager
{
    private readonly ILogger<ProfileManager> _logger;
    private readonly IOptions<ApplicationConfig> _options;
    private readonly ILogPropertyMgr _propMgr;
    private readonly IProfileLoader _profileLoader;
    private readonly IWrapWebDriver _wrapDriver;
    private readonly string _encryptionKey;

    public ProfileManager(ILogger<ProfileManager> logger,
                          IOptions<ApplicationConfig> options,
                          ILogPropertyMgr propMgr,
                          IProfileLoader profileLoader)
    {
        _logger = logger;
        _options = options;
        _propMgr = propMgr;
        _profileLoader = profileLoader;
    }

    public ChromeOptions BuildChromeOptions()
    {
        var chromeOpt = new ChromeOptions();

        try
        {
            bool useProfile = _options.Value.BrowserConfig.EnablePersistentProfile;

            if (useProfile)
            {
                var profileContext = _profileLoader.DequeueProfilePath();

                chromeOpt.AddArgument($"--user-data-dir={profileContext.UserDataDir}");
                chromeOpt.AddArgument($"--profile-directory={profileContext.ProfileDir}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}: Error building ChromeOptions.", nameof(BuildChromeOptions));
        }

        return chromeOpt;
    }

    private string GetProfilePath()
    {
        var result = string.Empty;

        if (NavigateToVersionPage().Success)
        {
            try
            {
                var xpath = "//*[@id='profile_path']";
                var profilePath = _wrapDriver.GetElement(By.XPath(xpath)).GetAttribute("innerText");

                if (!string.IsNullOrEmpty(profilePath))
                {
                    result = profilePath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method}: Error getting browser context directory path.", nameof(GetProfilePath));
            }
        }

        return result;
    }

    private string GetUserDataPath(string profilePath)
    {
        var result = string.Empty;

        if (!string.IsNullOrEmpty(profilePath))
        {
            var lastBackslashIndex = profilePath.LastIndexOf("\\");
            if (lastBackslashIndex > -1)
            {
                result = profilePath.Substring(0, lastBackslashIndex);
            }
            else
            {
                _logger.LogError("{Method}: Error getting user data path from {ProfilePath}.", nameof(GetUserDataPath), profilePath);
            }
        }

        return result;
    }

    private IOperationResult NavigateToVersionPage()
    {
        IOperationResult result = OperationResult.Ok();

        try
        {
            if (_wrapDriver.StartDriver().Success)
            {
                _wrapDriver.NavigatePage("chrome://version");
            }
            else
            {
                throw new Exception("Failed to start driver.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}: Error navigating to 'chrome://version'.", nameof(GetProfilePath));
            result = OperationResult.Fail();
        }

        return result;
    }

    public IOperationResult PurgeUserData(string profilePath)
    {
        throw new NotImplementedException();
    }
}
