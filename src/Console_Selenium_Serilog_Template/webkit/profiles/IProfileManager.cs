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
using OpenQA.Selenium.Chrome;

namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

/// <summary>
/// This interface is responsible for managing browser-profile operations.
/// </summary>
public interface IProfileManager
{
    /// <summary>
    /// Purges the user-data and profile from the browser context.
    /// </summary>
    /// <param name="profilePath">The path to the profile to purge.</param>
    /// <returns>An operation result indicating the outcome of the purge.</returns>
    IOperationResult PurgeUserData(string profilePath);

    /// <summary>
    /// Builds Chrome options.
    /// </summary>
    /// <returns>A ChromeOptions object configured with the desired settings.</returns>
    ChromeOptions BuildChromeOptions();
}
