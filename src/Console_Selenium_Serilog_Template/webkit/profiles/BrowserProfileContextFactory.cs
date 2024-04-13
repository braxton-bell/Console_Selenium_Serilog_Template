﻿/*
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

public class BrowserProfileContextFactory : IBrowserProfileContextFactory
{
    private readonly ILogger<BrowserProfileContextFactory> _logger;
    private readonly IOptions<ApplicationConfig> _options;
    private readonly ILogPropertyMgr _propMgr;

    public BrowserProfileContextFactory(
        ILogger<BrowserProfileContextFactory> logger,
        IOptions<ApplicationConfig> options,
        ILogPropertyMgr propMgr)
    {
        _logger = logger;
        _options = options;
        _propMgr = propMgr;
    }

    public IBrowserProfileContext CreateBrowserProfileContext(string userDataDir, string profileDir)
    {
        return new BrowserProfileContext(userDataDir, profileDir);
    }
}
