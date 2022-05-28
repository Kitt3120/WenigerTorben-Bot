using System;

namespace WenigerTorbenBot.Utils;

public class PlatformUtils
{
    public static PlatformID GetOSPlatform()
    {
        if (OperatingSystem.IsWindows())
            return PlatformID.Win32NT;
        else if (OperatingSystem.IsMacOS())
            return PlatformID.Unix;
        else if (OperatingSystem.IsLinux())
            return PlatformID.Unix;
        else
            return PlatformID.Other;
    }

    public static bool IsOSPlatformSupported()
    {
        return GetOSPlatform() != PlatformID.Other;
    }
}