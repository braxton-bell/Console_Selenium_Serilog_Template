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


using Microsoft.Extensions.Configuration;

namespace Console_Selenium_Serilog_Template.Config;

public class ConfigBuilderWrap
{
    private readonly IConfiguration _configuration;

    public ConfigBuilderWrap()
    {
        _configuration = BuildStartupConfig();
    }

    public IConfiguration Configuration
    {
        get { return _configuration; }
    }

    // The appsettings.json must be copied to the build directory for this to work.
    private IConfiguration BuildStartupConfig()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder
            .SetBasePath($"{Directory.GetCurrentDirectory()}/config/settings")
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), true);
        // https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-dotnet-core-app?tabs=windowscommandprompt
        // .AddAzureAppConfiguration(Environment.GetEnvironmentVariable("ConnectionString"));

        return configBuilder.Build();
    }
}
