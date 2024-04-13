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

/// <summary>
/// Represents the outcome of an operation with a success flag, an error message, and an exception detail.
/// This class is used as the return type for methods where it is necessary to understand if the 
/// operation was successful or if it failed, providing context about the failure when needed.
/// The properties of this class are read-only and set upon creation, making instances of this class immutable 
/// and thread-safe. This immutability is essential for concurrent operations, ensuring that the result 
/// state cannot be altered after it has been constructed, which allows for reliable and predictable behavior 
/// when handling results across different threads.
/// </summary>
/// <remarks>
/// Instances of this class should be created using the provided static factory methods: 
/// <see cref="OperationResult.Ok"/> to indicate success and <see cref="OperationResult.Fail"/> to indicate failure.
/// By doing so, it maintains consistency in how operation results are generated and consumed throughout the application.
/// </remarks>
public class OperationResult : IOperationResult
{
    public bool Success { get; }
    public string ErrorMessage { get; }
    public Exception Exception { get; }

    private OperationResult(bool success, string errorMessage = "", Exception exception = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static OperationResult Ok()
    {
        return new OperationResult(true);
    }

    public static OperationResult Fail(string message = "", Exception ex = null)
    {
        return new OperationResult(false, message, ex);
    }
}