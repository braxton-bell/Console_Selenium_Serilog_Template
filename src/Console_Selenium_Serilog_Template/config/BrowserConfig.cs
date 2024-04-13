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


using Console_Selenium_Serilog_Template.Webkit;

namespace Console_Selenium_Serilog_Template.Config;

public class BrowserConfig
{
    public BrowserConfig()
    {
    }

    // Constructor for unit testing
    public BrowserConfig(WebDriverType browserProfile,
                         bool enablePersistentProfile,
                         string globalProfilePath,
                         string seleniumGridHub,
                         bool enableProxy,
                         ProxySettings proxySettings,
                         string pluginPath,
                         bool enableAntiCaptchaExt,
                         AntiCaptchaExtSettings antiCaptchaExtSettings)
    {
        BrowserProfile = browserProfile;
        EnablePersistentProfile = enablePersistentProfile;
        GlobalProfilePath = globalProfilePath;
        SeleniumGridHub = seleniumGridHub;
        EnableProxy = enableProxy;
        ProxySettings = proxySettings;
        PluginPath = pluginPath;
        EnableAntiCaptchaExt = enableAntiCaptchaExt;
        AntiCaptchaExtSettings = antiCaptchaExtSettings;
    }

    public WebDriverType BrowserProfile { get; set; }
    public bool EnablePersistentProfile { get; set; }
    public string GlobalProfilePath { get; set; }
    public string SeleniumGridHub { get; set; }
    public bool EnableProxy { get; set; }
    public ProxySettings ProxySettings { get; set; }
    public string PluginPath { get; set; }
    public bool EnableAntiCaptchaExt { get; set; }
    public AntiCaptchaExtSettings AntiCaptchaExtSettings { get; set; }
}

public class ProxySettings
{
    public string HttpProxy { get; set; }
    public string SslProxy { get; set; }
}

public class AntiCaptchaExtSettings
{
    public string FileName { get; set; }
    public string Url { get; set; }
    public string Key { get; set; }
}
