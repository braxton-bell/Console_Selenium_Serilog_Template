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
using Console_Selenium_Serilog_Template.Sandbox;
using Console_Selenium_Serilog_Template.Utilities;
using Console_Selenium_Serilog_Template.Webkit;
using Console_Selenium_Serilog_Template.Webkit.Profiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Console_Selenium_Serilog_Template;

public class Startup
{
    private readonly ConfigBuilderWrap _configBuilder;
    private readonly IHost _host;
    private readonly IOptions<ApplicationConfig> _options;
    private readonly ILogger<Startup> _logger;
    private readonly ILogPropertyMgr _propMgr;

    public Startup()
    {
        // Build the configuration, it should fail early if there are any issues.
        _configBuilder = new ConfigBuilderWrap();

        // Create the logger.
        StartLogger();

        // Add the host container for the application.
        var hostContainer = BuildHostContainer();
        if (hostContainer == null)
        {
            Log.Logger.Fatal("Bootstrap: The host container is null.");
            Log.CloseAndFlush();
            throw new Exception("Bootstrap: The host container is null.");
        }
        else
        {
            _host = hostContainer;
        }

        // Add the application options from the host container.
        _options = _host.Services.GetService<IOptions<ApplicationConfig>>();

        // Set the logger for the Bootstrap class. From this point on, the logger can be used in the Bootstrap class.
        _logger = _host.Services.GetService<ILogger<Startup>>();

        // Create the property manager for the logger.
        _propMgr = new LogPropertyMgr();

        // Uncomment to add a property to the logger.
        // _propMgr.Add("SomeKey", "SomeValue");
    }

    private void StartLogger()
    {
        var logConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(_configBuilder.Configuration);
        // .Enrich.FromLogContext()
        // .WriteTo.Console()
        // .WriteTo.File($"{Directory.GetCurrentDirectory()}/Logs/log.txt")
        // .WriteTo.SQLite($"{Directory.GetCurrentDirectory()}/Logs/log.db");

        // Start the logger for the entire application. From this point on, the logger can be used in any class.
        Log.Logger = logConfig.CreateLogger();
    }

    private IHost BuildHostContainer()
    {
        IHost hostContainer = null;

        Log.Logger.Debug("Bootstrap: Initializing host container (InstanceId: {InstanceId})", Guid.NewGuid);

        try
        {
            hostContainer = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath($"{Directory.GetCurrentDirectory()}/config/settings");
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile(
                        $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                        optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), true);
                })
                .ConfigureServices((context, services) =>
                {

                    // App Options
                    services.Configure<ApplicationConfig>(options =>
                    {
                        _configBuilder.Configuration.GetSection("AppConfig").Bind(options, c => c.BindNonPublicProperties = true);
                    });

                    // Sandbox
                    services.AddTransient<ISandbox, Sandbox.Sandbox>();

                    // Utilities
                    services.AddTransient<ILogPropertyMgr, LogPropertyMgr>();

                    // Webkit
                    services.AddTransient<IWrapWebDriverFactory, WrapWebDriverFactory>();
                    services.AddSingleton<IProfileLoader, ProfileLoader>();
                    services.AddTransient<IProfileManager, ProfileManager>();
                    services.AddTransient<IBrowserProfileContextFactory, BrowserProfileContextFactory>();


                })
                .UseSerilog()
                .Build();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Bootstrap: Error initializing the host container.");
            hostContainer = null;
        }

        return hostContainer;
    }

    public async Task<IOperationResult> StartAsync()
    {
        IOperationResult result = OperationResult.Ok();

        try
        {
            using (_logger.BeginScope(_propMgr.GetProperties()))
            {
                // Check if the application is running in sandbox mode.
                bool sandboxed = false;
                await Task.Run(() => sandboxed = GetSandbox());
                if (sandboxed) return result;

                // Run the app
                result = await RunAppAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}: Error in Start()", nameof(StartAsync));
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return result;
    }

    private bool GetSandbox()
    {
        bool hasSandbox = false;

        if (_options.Value.DevOptions.Sandbox)
        {
            hasSandbox = true;

            Log.Logger.Information("{Method}: Initializing Sandbox", nameof(GetSandbox));

            var pit = _host.Services.GetService<ISandbox>();
            pit.Play();

            _logger.LogInformation("Startup: Exiting Sandbox");
        }

        return hasSandbox;
    }

    private async Task<IOperationResult> RunAppAsync()
    {
        _logger.LogInformation("{Method}: Running App", nameof(RunAppAsync));

        // Await time-out
        await Task.Delay(1000);

        return OperationResult.Ok();
    }
}
