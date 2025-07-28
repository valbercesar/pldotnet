using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PlDotNET.Common
{
    /// <summary>
    /// Loads and provides access to runtime settings for PL/.NET from PostgreSQL's configuration,
    /// using lazy initialization and interop with the PostgreSQL backend.
    /// </summary>
    public partial class PlDotNETSettings
    {
        /// <summary>
        /// Stores lazy-loaded PostgreSQL configuration settings by key.
        /// </summary>
        private readonly Dictionary<string, Lazy<string>> settings;

        /// <summary>
        /// Cached list of user assembly DLL paths to avoid repeated file system operations.
        /// </summary>
        private List<string> cachedUserAssemblyPaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlDotNETSettings"/> class.
        /// Preloads known configuration keys with deferred access via Lazy evaluation.
        /// </summary>
        public PlDotNETSettings()
        {
            settings = [];

            var settingsKeys = new[]
            {
                "pldotnet.always_nullable",
                "pldotnet.print_source_code",
                "pldotnet.save_source_code",
                "pldotnet.compile_fsharp_with_fcs",
                "pldotnet.verbose_level",
                "pldotnet.path_to_save_source_code",
                "pldotnet.path_to_temporary_files",
                "pldotnet.user_assemblies_directory",
                "pldotnet.user_assembly_paths",
            };

            foreach (var key in settingsKeys)
            {
                settings[key] = new Lazy<string>(() => GetPostgreSetting(key));
            }
        }

        /// <summary>
        /// Gets a value indicating whether all database values should be treated as nullable by default.
        /// </summary>
        public bool AlwaysNullable => ConvertToBoolean(settings["pldotnet.always_nullable"].Value);

        /// <summary>
        /// Gets a value indicating whether the generated source code should be printed to the PostgreSQL log.
        /// </summary>
        public bool PrintSourceCode => ConvertToBoolean(settings["pldotnet.print_source_code"].Value);

        /// <summary>
        /// Gets a value indicating whether the generated source code should be saved to disk.
        /// </summary>
        public bool SaveSourceCode => ConvertToBoolean(settings["pldotnet.save_source_code"].Value);

        /// <summary>
        /// Gets a value indicating whether F# compilation should use the FCS (F# Compiler Services) backend.
        /// </summary>
        public bool CompileFSharpWithFCS => ConvertToBoolean(settings["pldotnet.compile_fsharp_with_fcs"].Value);

        /// <summary>
        /// Gets the configured verbosity level for diagnostics or logging output.
        /// </summary>
        public int VerboseLevel => int.Parse(settings["pldotnet.verbose_level"].Value);

        /// <summary>
        /// Gets the file system path where generated source code should be saved.
        /// </summary>
        public string PathToSaveSourceCode => settings["pldotnet.path_to_save_source_code"].Value;

        /// <summary>
        /// Gets the directory path used to store temporary files created during compilation or execution.
        /// </summary>
        public string PathToTemporaryFiles => settings["pldotnet.path_to_temporary_files"].Value;

        /// <summary>
        /// Gets the directory path where user assemblies are located.
        /// </summary>
        public string UserAssembliesDirectory => settings["pldotnet.user_assemblies_directory"].Value;

        /// <summary>
        /// Gets the comma-separated list of user assembly paths.
        /// </summary>
        public string UserAssemblyPaths => settings["pldotnet.user_assembly_paths"].Value;

        /// <summary>
        /// Gets a list of all user assembly DLL paths, combining paths from both the user assemblies directory
        /// and explicitly specified assembly paths. Results are cached for performance.
        /// </summary>
        public List<string> GetUserAssemblyDllPaths()
        {
            // Return cached results if available
            if (cachedUserAssemblyPaths != null)
            {
                return cachedUserAssemblyPaths;
            }

            // Build the assembly paths list
            cachedUserAssemblyPaths = BuildUserAssemblyPaths();

            Elog.Info($"Found {cachedUserAssemblyPaths.Count} user assembly DLL paths.");
            // Log the paths for debugging
            foreach (var path in cachedUserAssemblyPaths)
            {
                Elog.Info($"User assembly DLL path: {path}");
            }

            return cachedUserAssemblyPaths;
        }

        /// <summary>
        /// Calls the PostgreSQL backend to retrieve the value of a server configuration setting by name.
        /// </summary>
        /// <param name="settingName">The name of the PostgreSQL configuration setting to retrieve.</param>
        /// <returns>
        /// A pointer to an ANSI string representing the setting's value, or <see cref="IntPtr.Zero"/> if not found.
        /// </returns>
        [LibraryImport("@PKG_LIBDIR/pldotnet.so", StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr pldotnet_GetPostgresSetting(string settingName);

        /// <summary>
        /// Converts a PostgreSQL "on"/"off" setting value to a boolean.
        /// </summary>
        /// <param name="value">The setting value, expected to be "on" or "off" (case-insensitive).</param>
        /// <returns><c>true</c> if the value is "on"; otherwise, <c>false</c>.</returns>
        private static bool ConvertToBoolean(string value)
        {
            return value?.ToLower() == "on";
        }

        /// <summary>
        /// Retrieves a PostgreSQL configuration setting by name via interop.
        /// </summary>
        /// <param name="settingName">The name of the PostgreSQL configuration setting.</param>
        /// <returns>
        /// The string representation of the setting's value, or <c>null</c> if the value is not found.
        /// </returns>
        private static string GetPostgreSetting(string settingName)
        {
            IntPtr resultPtr = pldotnet_GetPostgresSetting(settingName);
            if (resultPtr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringAnsi(resultPtr);
        }

        /// <summary>
        /// Builds the list of user assembly DLL paths from configuration settings.
        /// </summary>
        private List<string> BuildUserAssemblyPaths()
        {
            var assemblyPaths = new List<string>();

            // Add DLL files from the user assemblies directory
            AddAssembliesFromDirectory(assemblyPaths);

            // Add explicitly specified assembly paths (comma-separated)
            AddExplicitAssemblyPaths(assemblyPaths);

            // Remove duplicates and return
            return assemblyPaths.Distinct().ToList();
        }

        /// <summary>
        /// Adds DLL files from the user assemblies directory to the assembly paths list.
        /// </summary>
        private void AddAssembliesFromDirectory(List<string> assemblyPaths)
        {
            var userAssembliesDir = UserAssembliesDirectory;
            Elog.Info($"Loading user assemblies from directory: {userAssembliesDir}");
            if (string.IsNullOrEmpty(userAssembliesDir) || !System.IO.Directory.Exists(userAssembliesDir))
            {
                return;
            }

            try
            {
                assemblyPaths.AddRange(System.IO.Directory.GetFiles(userAssembliesDir, "*.dll", System.IO.SearchOption.TopDirectoryOnly));
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Elog.Warning($"User assemblies directory '{userAssembliesDir}' not found. No user assemblies will be loaded.");
            }
            catch (UnauthorizedAccessException)
            {
                Elog.Warning($"Access denied to user assemblies directory '{userAssembliesDir}'. No user assemblies will be loaded.");
            }
            catch (Exception ex)
            {
                Elog.Error($"Error accessing user assemblies directory '{userAssembliesDir}': {ex.Message}");
            }
        }

        /// <summary>
        /// Adds explicitly specified assembly paths from the comma-separated setting to the assembly paths list.
        /// </summary>
        private void AddExplicitAssemblyPaths(List<string> assemblyPaths)
        {
            var userAssemblyPathsSetting = UserAssemblyPaths;
            if (string.IsNullOrEmpty(userAssemblyPathsSetting))
            {
                return;
            }

            foreach (var path in userAssemblyPathsSetting.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedPath = path.Trim();
                if (!string.IsNullOrEmpty(trimmedPath) && System.IO.File.Exists(trimmedPath))
                {
                    assemblyPaths.Add(trimmedPath);
                }
            }
        }
    }
}
