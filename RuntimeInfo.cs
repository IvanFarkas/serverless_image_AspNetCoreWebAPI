using System.Runtime.InteropServices;

namespace serverless_image_AspNetCoreWebAPI;

public static class RuntimeInfo
{
  public static OSPlatform GetOsPlatform()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return OSPlatform.OSX;
    }
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      return OSPlatform.Linux;
    }
    if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
    {
      return OSPlatform.FreeBSD;
    }
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return OSPlatform.Windows;
    }
    return OSPlatform.Create("Unknown");
  }

  public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
  public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
  public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
  public static bool IsFreeBSD() => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
  public static string RuntimeIdentifier() => RuntimeInformation.RuntimeIdentifier;
  public static Architecture ProcessArchitecture() => RuntimeInformation.ProcessArchitecture;
  public static Architecture OsArchitecture() => RuntimeInformation.OSArchitecture;
  public static string OsDescription() => RuntimeInformation.OSDescription;
  public static string FrameworkDescription() => RuntimeInformation.FrameworkDescription;
}
