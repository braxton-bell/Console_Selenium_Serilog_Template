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

namespace Console_Selenium_Serilog_Template.Webkit;

public class WrapWebDriverFactory : IWrapWebDriverFactory
{
    private readonly IServiceProvider _services;

    public WrapWebDriverFactory(IServiceProvider services)
    {
        _services = services;
    }

    private IServiceProvider Services => _services;

    public IWrapWebDriver Create()
    {
        var logger = (ILogger<WrapWebDriver>)Services.GetService(typeof(ILogger<WrapWebDriver>));
        var propMgr = (ILogPropertyMgr)Services.GetService(typeof(ILogPropertyMgr));
        var options = (IOptions<ApplicationConfig>)Services.GetService(typeof(IOptions<ApplicationConfig>));
        var profileMgr = (IProfileManager)Services.GetService(typeof(IProfileManager));

        return new WrapWebDriver(
            logger: logger,
            propMgr: propMgr,
            options: options,
            profileMgr: profileMgr
        );
    }
}