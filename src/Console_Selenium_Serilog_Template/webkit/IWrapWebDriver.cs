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


using Console_Selenium_Serilog_Template.Utilities;
using OpenQA.Selenium;

namespace Console_Selenium_Serilog_Template.Webkit;

public interface IWrapWebDriver : IDisposable
{
    IWebDriver WebDriver { get; }

    string BrowserSessionId { get; }

    bool CheckIsHealthyDriver();

    void NavigatePage(string url, double timeout = 10, bool matchUrl = true, bool recursiveUrl = true, bool requirePageLoad = true, bool forceReload = false);

    void ClickElement(By driverBy, int timeout, bool requirePageLoad = true);

    bool AssertIsClickable(By driverBy, double timeout = 10);

    bool AssertIsClickable(IWebElement element, double timeout = 10);

    Task<bool> AssertIsDisplayedAsync(By driverBy, double timeout = 10);

    bool AssertIsDisplayed(By driverBy, double timeout = 10);

    bool AssertIsDisplayed(IWebElement element, double timeout = 10);

    bool AssertIsNotClickable(By driverBy, double timeout = 10);

    bool AssertIsNotClickable(IWebElement element, double timeout = 10);

    bool AssertIsNotDisplayed(By driverBy, double timeout = 10);

    bool AssertIsNotDisplayed(IWebElement element, double timeout = 10);

    IWebElement GetElement(By driverBy, double timeoutSeconds = 10);

    IReadOnlyCollection<IWebElement> GetElements(By driverBy);

    bool IsPageReady(string uri, double timeout = 10, bool recursiveUri = true);

    bool TryIsPageReady(string uriParam, double timeout = 10, bool recursiveUri = true, bool forceReload = false);

    IOperationResult StartDriver();

    IOperationResult RestartDriver(double delaySeconds = 5);

    Task StartLoggingAsync(CancellationToken cancellationToken);

    IOperationResult StopDriver();
}
