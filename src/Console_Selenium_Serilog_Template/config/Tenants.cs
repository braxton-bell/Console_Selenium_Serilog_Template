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

public class Tenants
{
    public Tenants()
    {
    }

    // Constructor for unit testing
    public Tenants(string tenantProfile, TenantDetails tenant1, TenantDetails tenant2)
    {
        TenantProfile = tenantProfile;
        Tenant1 = tenant1;
        Tenant2 = tenant2;
    }

    public string TenantProfile { get; set; }
    public TenantDetails Tenant1 { get; set; }
    public TenantDetails Tenant2 { get; set; }

    public TenantDetails ActiveTenant
    {
        get
        {
            return TenantProfile == "Tenant1" ? Tenant1 : Tenant2;
        }
    }
}
