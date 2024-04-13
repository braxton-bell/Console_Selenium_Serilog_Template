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
using Console_Selenium_Serilog_Template.Webkit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Console_Selenium_Serilog_Template.Sandbox;

public interface ISandbox
{
    void Play();
}

public class Sandbox : ISandbox
{
    private readonly ILogger<Sandbox> _logger;
    private readonly IServiceProvider _services;
    private readonly IOptions<ApplicationConfig> _options;

    public Sandbox(ILogger<Sandbox> logger, IServiceProvider services, IOptions<ApplicationConfig> options)
    {
        _logger = logger;
        _services = services;
        _options = options;

        _logger.LogInformation("Entering development sandbox... ");
    }

    public void Play()
    {
        //PrintNum();

        LaunchBrowser();
    }

    private void LaunchBrowser()
    {
        // This code demonstrates how to launch a browser and navigate to a page.

        _logger.LogInformation("Launching browser...");
        
        
        // get the browser factory
        IWrapWebDriverFactory factory = (IWrapWebDriverFactory)_services.GetService(typeof(IWrapWebDriverFactory));

        // create the browser
        IWrapWebDriver browser = factory.Create();
        
        // start the browser
        browser.StartDriver();
        
        // navigate to the page
        browser.NavigatePage("https://api.chucknorris.io/");

        // get the title of the page and log it
        string title = browser.WebDriver.Title;
        _logger.LogInformation("Title of the page: {title}", title);

        // pause for 5 seconds
        Thread.Sleep(5000);

        // stop the browser
        browser.StopDriver();
    }

    private async Task<string> DoSomethingTestAsync()
    {
        var t = Task.Run(() =>
        {
            Thread.Sleep(12000);
            return "test";
        });

        string myString = await t;

        return myString;
    }

    private void PrintNum()
    {
        int max = 10000;
        int counter = 0;

        while (counter < max)
        {
            _logger.LogInformation("Counter: {counterVar}", counter);
            Thread.Sleep(1000);
            counter++;
        }

        _logger.LogInformation("Counter: {counterVar}", counter);
    }

    private void PrintNum_2()
    {
        int max = 10;
        int counter = 0;

        var myString = DoSomethingTestAsync();

        while (counter < max)
        {
            _logger.LogInformation($"Counter: {counter}");
            Thread.Sleep(1000);
            counter++;
            if (myString.IsCompleted)
                _logger.LogInformation(myString.Result);
        }

        if (myString.Wait(1000))
        {
            _logger.LogInformation("Job Completed");
        }
        else
        {
            _logger.LogInformation("Time out!");
        }
    }
}
