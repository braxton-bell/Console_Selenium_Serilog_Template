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


namespace Console_Selenium_Serilog_Template.Utilities;

public class ExceptionTracker : IExceptionTracker
{
    // Dictionary to keep track of exception types and their details (count and an example exception)
    private readonly Dictionary<string, (int Count, Exception ExampleException)> exceptionDetails = new();

    /// <summary>
    /// Tracks an exception, incrementing the count and storing an example if it's the first occurrence.
    /// </summary>
    /// <param name="ex">The exception to track.</param>
    public void TrackException(Exception ex)
    {
        string exType = ex.GetType().ToString();
        if (!exceptionDetails.ContainsKey(exType))
        {
            exceptionDetails[exType] = (0, ex);
        }
        var currentDetails = exceptionDetails[exType];
        exceptionDetails[exType] = (currentDetails.Count + 1, currentDetails.ExampleException);
    }

    /// <summary>
    /// Gets the details of all tracked exceptions.
    /// </summary>
    /// <returns>A dictionary of exception types and their details.</returns>
    public IReadOnlyDictionary<string, (int Count, Exception ExampleException)> GetExceptionDetails()
    {
        return exceptionDetails;
    }

    /// <summary>
    /// Factory method for creating an instance of ExceptionTracker.
    /// </summary>
    /// <returns>An instance of ExceptionTracker.</returns>
    public static ExceptionTracker Create()
    {
        return new ExceptionTracker();
    }
}
