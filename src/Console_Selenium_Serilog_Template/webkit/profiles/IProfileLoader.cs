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

namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

/// <summary>
/// This interface is responsible for handling the loading of browser profiles from the Duxsl app context (application data folder).
/// Objects of this type will read a list of profiles from a directory. It will maintain a threadsafe queue of profiles to be used by the IProfileManager.
/// If a profile is available, it will provide the IProfileManager with an IBrowserProfileContext object.
/// Objects of this interface are consumed by the IProfileManager.
/// Responsibilities:
/// - Read a list of profiles from a directory
/// - Maintain a threadsafe queue of profiles to be used by the IProfileManager
/// - Provide the IProfileManager with an IBrowserProfileContext object if a profile is available
/// </summary>
public interface IProfileLoader
{
    int QueueCount { get; }

    /// <summary>
    /// Enqueues a new profile into the internal queue.
    /// </summary>
    IOperationResult EnqueueProfile(IBrowserProfileContext browserContext);

    /// <summary>
    /// Dequeues a profile from the internal queue and provides it for use.
    /// </summary>
    /// <returns>Profile data as an IBrowserProfileContext object, or null if the queue is empty.</returns>
    IBrowserProfileContext DequeueProfilePath();
}
