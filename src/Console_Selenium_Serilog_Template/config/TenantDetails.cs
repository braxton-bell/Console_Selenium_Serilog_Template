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


namespace Console_Selenium_Serilog_Template.Config;

public class TenantDetails
{
    public TenantDetails()
    {
    }

    // Constructor for unit testing
    public TenantDetails(Guid tenantId,
                         Guid siteId,
                         string name,
                         long userId,
                         string username,
                         string password,
                         string cnameUrl,
                         string redirectUrl)
    {
        TenantId = tenantId;
        SiteId = siteId;
        Name = name;
        UserId = userId;
        Username = username;
        Password = password;
        CnameUrl = cnameUrl;
        RedirectUrl = redirectUrl;
    }

    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string Name { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string CnameUrl { get; set; }
    public string RedirectUrl { get; set; }
}
