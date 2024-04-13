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
using Console_Selenium_Serilog_Template.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Console_Selenium_Serilog_Template.Webkit.Profiles;

/// <summary>
/// Manages loading and queuing of browser profiles for the Webkit engine.
/// </summary>
/// <remarks>
/// This class is responsible for managing browser profiles, including loading existing profiles into a queue
/// and creating new profiles as needed. Key responsibilities and behaviors include:
/// <list type="bullet">
/// <item>
/// <description>Loading Profiles: On initialization, it loads existing profiles from a specified global profile path into a concurrent queue. This path is configured in <c>appsettings.json</c> and is associated with a specific tenant ID and site ID.</description>
/// </item>
/// <item>
/// <description>Queue Management: Profiles are enqueued and dequeued as needed, supporting concurrent operations to manage the lifecycle of browser profiles.</description>
/// </item>
/// <item>
/// <description>Profile Creation: When no existing profiles are available to dequeue, a new profile directory is created and enqueued, ensuring a continuous supply of browser profiles for processing tasks.</description>
/// </item>
/// <item>
/// <description>Error Handling: The class includes comprehensive error handling to log and manage exceptions related to file access, directory existence, and profile management operations.</description>
/// </item>
/// </list>
/// Dependencies include ILogger, IOptions for application configuration, and an IBrowserProfileContextFactory for creating profile contexts.
/// </remarks>
public class ProfileLoader : IProfileLoader
{
    private readonly ConcurrentQueue<IBrowserProfileContext> _profileQueue;
    private readonly ILogger<ProfileLoader> _logger;
    private readonly IOptions<ApplicationConfig> _options;
    private readonly ILogPropertyMgr _propMgr;
    private readonly IBrowserProfileContextFactory _contextFactory;
    private readonly string _globalPath;
    private readonly Guid _tenantId;
    private readonly Guid _siteId;
    private List<int> _profilePathIndexes;

    public int QueueCount => _profileQueue.Count;

    private List<int> ProfilePathIndexes
    {
        get
        {
            lock (_profilePathIndexes)
            {
                return _profilePathIndexes;
            }
        }
        set
        {
            lock (_profilePathIndexes)
            {
                _profilePathIndexes = value;
            }
        }
    }

    public ProfileLoader(
        ILogger<ProfileLoader> logger,
        IOptions<ApplicationConfig> options,
        ILogPropertyMgr propMgr,
        IBrowserProfileContextFactory contextFactory)
    {
        _profileQueue = new ConcurrentQueue<IBrowserProfileContext>();
        _logger = logger;
        _options = options;
        _propMgr = propMgr;
        _contextFactory = contextFactory;
        _profilePathIndexes = new List<int>();
        _globalPath = _options.Value.BrowserConfig.GlobalProfilePath;
        _tenantId = _options.Value.Tenants.ActiveTenant.TenantId;
        _siteId = _options.Value.Tenants.ActiveTenant.SiteId;

        // Load the profiles from the profiles directory.
        if (!LoadProfiles().Success)
        {
            throw new Exception("Failed to load profiles.");
        }
    }

    /// <summary>
    /// Enqueues a new profile into the queue.
    /// </summary>
    /// <returns></returns>
    public IOperationResult EnqueueProfile(IBrowserProfileContext browserContext)
    {
        var result = OperationResult.Ok();
        _profileQueue.Enqueue(browserContext);
        return result;
    }

    /// <summary>
    /// Dequeues a profile path from the queue and provides it for use.
    /// </summary>
    public IBrowserProfileContext DequeueProfilePath()
    {
        IBrowserProfileContext browserProfile = null;

        try
        {
            if (!_profileQueue.TryDequeue(out browserProfile))
            {
                // Create a new profile context.
                browserProfile = CreateNewProfileDir();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} Exception: {Message}", nameof(DequeueProfilePath), ex.Message);
        }

        return browserProfile;
    }

    private IOperationResult LoadProfiles()
    {
        var result = OperationResult.Ok();

        try
        {
            var userDataDir = GetUserDataDir();
            var profileDirectories = GetProfileDirectories(userDataDir);

            if (profileDirectories.Any())
            {
                foreach (var profileDir in profileDirectories)
                {
                    var profileDirName = Path.GetFileName(profileDir);

                    // Parse the profile path index.
                    ParseProfilePathIndex(profileDirName);

                    // Resolves issue with shared UserDataDir causing browser crash.
                    var trueUserDataDir = Path.Combine(userDataDir, profileDirName);
                    var trueProfileDir = "Default";

                    var browserProfileContext = _contextFactory.CreateBrowserProfileContext(trueUserDataDir, trueProfileDir);

                    _profileQueue.Enqueue(browserProfileContext);
                }
            }
            else
            {
                _logger.LogDebug("{Method}: No profiles found in {UserDataDir}.", nameof(LoadProfiles), userDataDir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} Exception: {Message}", nameof(LoadProfiles), ex.Message);
            result = OperationResult.Fail();
        }

        return result;
    }

    private IBrowserProfileContext CreateNewProfileDir()
    {
        IBrowserProfileContext browserProfileContext = null;

        try
        {
            var userDataDir = GetUserDataDir();
            string profileDir = null;
            string trueUserDataDir = null;
            string trueProfileDir = null;

            // Important: 'lock'
            // This is the only place where the _profilePathIndexes is accessed
            // outside of the ProfilePathIndexes property.
            lock (_profilePathIndexes)
            {
                // Get the next profile path index.
                int nextIndex;
                if (_profilePathIndexes.Any())
                {
                    nextIndex = _profilePathIndexes.Max() + 1;
                }
                else
                {
                    nextIndex = 1;
                }

                // Catch null/empty site id.
                if (_siteId == Guid.Empty)
                {
                    throw new Exception("Site id is null or empty.");
                }

                // Create the profile directory.
                profileDir = $"se_profile_{_siteId}_{nextIndex}";

                // Resolves issue with shared UserDataDir causing browser crash.
                trueUserDataDir = Path.Combine(userDataDir, profileDir);
                trueProfileDir = "Default";

                Directory.CreateDirectory(trueUserDataDir);
                Directory.CreateDirectory(Path.Combine(trueUserDataDir, trueProfileDir));

                // Add the index to the _profilePathIndexes list
                _profilePathIndexes.Add(nextIndex);
            }

            // Create the profile context.
            browserProfileContext = _contextFactory.CreateBrowserProfileContext(trueUserDataDir, trueProfileDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method}: Error creating new profile directory.", nameof(CreateNewProfileDir));
        }

        return browserProfileContext;
    }

    private void ParseProfilePathIndex(string profileDir)
    {
        // Extract the index from the directory name
        var indexString = profileDir.Substring(profileDir.LastIndexOf("_") + 1);

        if (int.TryParse(indexString, out var dirIndex))
        {
            // Add the index to the _profilePathIndexes list
            _profilePathIndexes.Add(dirIndex);
        }
        else
        {
            // Handle the case where the index is not an integer
            _logger.LogWarning("Failed to parse index for directory: {Profile_Directory}", profileDir);
        }
    }

    private string GetUserDataDir()
    {
        var userDataDir = Path.Combine(_globalPath, _tenantId.ToString());

        // catch null/empty user data dir.
        if (string.IsNullOrEmpty(userDataDir))
        {
            throw new Exception("User data directory is null or empty.");
        }

        // verify the user data dir exists.
        if (!Directory.Exists(userDataDir))
        {
            throw new Exception("User data directory does not exist.");
        }

        return userDataDir;
    }

    private string[] GetProfileDirectories(string userDataDir)
    {
        return Directory.GetDirectories(userDataDir, "se_profile*");
    }
}
