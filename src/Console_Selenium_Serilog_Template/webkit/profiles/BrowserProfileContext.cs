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


namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

public class BrowserProfileContext : IBrowserProfileContext
{
    private readonly string _userDataDir;
    private readonly string _profileDir;

    public BrowserProfileContext(string userDataDir, string profileDir)
    {
        if (string.IsNullOrEmpty(userDataDir))
        {
            throw new ArgumentException($"'{nameof(userDataDir)}' cannot be null or empty.", nameof(userDataDir));
        }

        if (string.IsNullOrEmpty(profileDir))
        {
            throw new ArgumentException($"'{nameof(profileDir)}' cannot be null or empty.", nameof(profileDir));
        }

        _userDataDir = userDataDir;
        _profileDir = profileDir;
    }

    public string UserDataDir => _userDataDir;

    public string ProfileDir => _profileDir;
}
