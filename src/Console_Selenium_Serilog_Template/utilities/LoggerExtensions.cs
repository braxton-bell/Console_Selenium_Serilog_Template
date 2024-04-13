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


using Microsoft.Extensions.Logging;

namespace Console_Selenium_Serilog_Template.Utilities;

/// <summary>
/// Extension methods for ILogger interface to enhance logging capabilities.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs the count and details of each type of exception tracked by the ExceptionTracker.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="tracker">The ExceptionTracker instance containing the tracked exceptions.</param>
    /// <param name="context">The context in which the exceptions occurred (usually the method name).</param>
    public static void LogExceptionCounts(this ILogger logger, ExceptionTracker tracker, string context)
    {
        foreach (var kvp in tracker.GetExceptionDetails())
        {
            var exceptionType = kvp.Key;
            var count = kvp.Value.Count;
            var exampleException = kvp.Value.ExampleException;

            logger.LogWarning($"Exception of type {exceptionType} occurred {count} times in {context}. Example exception: {exampleException}");
        }
    }
}
