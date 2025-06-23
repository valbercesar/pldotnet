using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PlDotNET.Common
{
    public class PldotnetSettings
    {
        private readonly Dictionary<string, Lazy<string>> settings;

        public PldotnetSettings()
        {
            settings = new Dictionary<string, Lazy<string>>();

            var settingsKeys = new[]
            {
                "pldotnet.always_nullable",
                "pldotnet.print_source_code",
                "pldotnet.save_source_code",
                "pldotnet.compile_fsharp_with_fcs",
                "pldotnet.verbose_level",
                "pldotnet.path_to_save_source_code",
                "pldotnet.path_to_temporary_files",
            };

            foreach (var key in settingsKeys)
            {
                settings[key] = new Lazy<string>(() => GetPostgreSetting(key));
            }
        }

        public bool AlwaysNullable => ConvertToBoolean(settings["pldotnet.always_nullable"].Value);

        public bool PrintSourceCode => ConvertToBoolean(settings["pldotnet.print_source_code"].Value);

        public bool SaveSourceCode => ConvertToBoolean(settings["pldotnet.save_source_code"].Value);

        public bool CompileFSharpWithFCS => ConvertToBoolean(settings["pldotnet.compile_fsharp_with_fcs"].Value);

        public string VerboseLevel => settings["pldotnet.verbose_level"].Value;

        public string PathToSaveSourceCode => settings["pldotnet.path_to_save_source_code"].Value;

        public string PathToTemporaryFiles => settings["pldotnet.path_to_temporary_files"].Value;

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        private static extern IntPtr pldotnet_GetPostgreSetting(string settingName);

        private static bool ConvertToBoolean(string value)
        {
            return value?.ToLower() == "on";
        }

        private string GetPostgreSetting(string settingName)
        {
            IntPtr resultPtr = pldotnet_GetPostgreSetting(settingName);
            if (resultPtr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringAnsi(resultPtr);
        }
    }
}
