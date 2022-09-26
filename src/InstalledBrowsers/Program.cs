using Microsoft.Win32;
using System.Diagnostics;

var keyLocations = new[]
{
    @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet",
    @"SOFTWARE\Clients\StartMenuInternet"
};

RegistryKey startMenuInternetKey = null;
foreach (var location in keyLocations)
{
    startMenuInternetKey = Registry.LocalMachine.OpenSubKey(location);
    if (startMenuInternetKey != null)
        break;
}

if (startMenuInternetKey == null)
{
    Console.WriteLine("Cannot find installed browsers in registry.");
    Console.ReadKey();
    return;
}

var browserNames = startMenuInternetKey.GetSubKeyNames();

var browserVersions = new Dictionary<string, string?>();

foreach (var browserName in browserNames)
{
    var browserKey = startMenuInternetKey.OpenSubKey(browserName);
    var name = browserKey?.GetValue(null)?.ToString();
    
    if (string.IsNullOrWhiteSpace(name) || browserVersions.ContainsKey(name))
        continue;

    var pathKey = browserKey.OpenSubKey(@"shell\open\command");
    var path =  pathKey?.GetValue(null)?.ToString()?.Trim('\"');
    if (string.IsNullOrWhiteSpace(path))
    {
        browserVersions.Add(name, "Unknown");
        continue;
    }

    try
    {
        var version = FileVersionInfo.GetVersionInfo(path).FileVersion;
        browserVersions.Add(name, version);
    }
    catch
    {
        browserVersions.Add(name, "Unknown");
    }
}

if (!browserVersions.Any())
{
    Console.WriteLine("Cannot find installed browsers in registry.");
    Console.ReadKey();
    return;
}

Console.WriteLine(string.Join(", ", browserVersions.Select(kvp => $"{kvp.Key} version: {kvp.Value ?? "Unknown"}")));
Console.ReadKey();