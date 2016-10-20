using System;
using System.Collections.Generic;
using System.Text;

namespace AppLauncher.Class
{
    public class WinOperatingSystem
    {
        public enum WindowsVersions
        {
            UnKnown,
            Win95,
            Win98,
            WinMe,
            WinNT3or4,
            Win2000,
            WinXP,
            WinServer2003,
            WinVista,
            Win7,
            Win8,
            MacOSX,
            Unix,
            Xbox
        };

        public static WindowsVersions GetCurrentWindowsVersion()
        {
            // Get OperatingSystem information from the system namespace.
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            // Determine the platform.
            if (osInfo.Platform.Equals(System.PlatformID.Win32Windows))
            {
                // Platform is Windows 95, Windows 98, Windows 98 Second Edition, or Windows Me.
                switch (osInfo.Version.Minor)
                {
                    case 0:
                        return WindowsVersions.Win95;
                    case 10:
                        return WindowsVersions.Win98;
                    case 90:
                        return WindowsVersions.WinMe;
                }
            }
            else if (osInfo.Platform.Equals(System.PlatformID.Win32NT))
            {
                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000, or Windows XP.
                switch (osInfo.Version.Major)
                {
                    case 3:
                    case 4:
                        return WindowsVersions.WinNT3or4;
                    case 5:
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                //name = "Windows 2000";
                                return WindowsVersions.Win2000;
                            case 1:
                                //name = "Windows XP";
                                return WindowsVersions.WinXP;
                            case 2:
                                //name = "Windows Server 2003";
                                return WindowsVersions.WinServer2003;
                        }
                        break;
                    case 6:
                        // Windows Vista or Windows Server 2008 (distinct by product type)
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                return WindowsVersions.WinVista;
                            case 1:
                                return WindowsVersions.Win7;
                            case 2:
                                return WindowsVersions.Win8;
                        }
                        break;
                }
            }
            else if (osInfo.Platform.Equals(System.PlatformID.Unix))
            {
                return WindowsVersions.Unix;
            }
            else if (osInfo.Platform.Equals(System.PlatformID.MacOSX))
            {
                return WindowsVersions.MacOSX;
            }
            else if (osInfo.Platform.Equals(PlatformID.Xbox))
            {
                return WindowsVersions.Xbox;
            }
            return WindowsVersions.UnKnown;
        }
    }
}