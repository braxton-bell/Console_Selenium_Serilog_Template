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


using System.Collections.Concurrent;

namespace Console_Selenium_Serilog_Template.Utilities;

/// <summary>
/// This class manages log properties in a thread-safe manner.
/// It allows adding and retrieving log properties to be used with logging operations.
/// </summary>
public class LogPropertyMgr : ILogPropertyMgr
{
    // Thread-safe dictionary to store log properties.
    private readonly ConcurrentDictionary<string, LogProperty> _logPropertyDict;

    public LogPropertyMgr()
    {
        // Initialize the dictionary.
        _logPropertyDict = new ConcurrentDictionary<string, LogProperty>();
    }

    /// <summary>
    /// Adds a log property with a dynamic value determined by a function.
    /// If the key already exists, the existing property is updated.
    /// </summary>
    public void Add(string key, Func<object> value)
    {
        var prop = new LogProperty(key, value);
        // Update or add the property as necessary.
        _logPropertyDict.AddOrUpdate(key, prop, (k, v) => prop);
    }

    /// <summary>
    /// Adds a log property with a static string value.
    /// If the key already exists, the existing property is updated.
    /// </summary>
    public void Add(string key, string value)
    {
        // Wrap the string in a function for consistency with the dictionary values.
        Func<object> funcValue = () => value;
        var prop = new LogProperty(key, funcValue);
        // Update or add the property as necessary.
        _logPropertyDict.AddOrUpdate(key, prop, (k, v) => prop);
    }

    /// <summary>
    /// Removes a log property by its key.
    /// If the key exists, the property is removed.
    /// If the key does not exist, nothing happens.
    /// </summary>
    public void Drop(string key)
    {
        _logPropertyDict.TryRemove(key, out _);
    }

    /// <summary>
    /// Retrieves all log properties as a dictionary.
    /// This is a snapshot of the current state of properties.
    /// </summary>
    public IReadOnlyDictionary<string, object> GetProperties()
    {
        // Convert the concurrent dictionary to a regular dictionary.
        return _logPropertyDict.ToDictionary(pair => pair.Key, pair => pair.Value.Value);
    }

    /// <summary>
    /// Nested private class representing a single log property.
    /// </summary>
    private class LogProperty
    {
        private readonly Func<object> _value;
        public string Key { get; }

        public object Value
        {
            get
            {
                // Invoke the function to get the current value, returning "Null" if it fails.
                try
                {
                    return _value.Invoke() ?? "Null";
                }
                catch
                {
                    return "Null";
                }
            }
        }

        /// <summary>
        /// Constructor to create a new log property with a key and a function to determine its value.
        /// </summary>
        public LogProperty(string scopeKey, Func<object> scopeValue)
        {
            Key = scopeKey;
            _value = scopeValue;
        }
    }
}
