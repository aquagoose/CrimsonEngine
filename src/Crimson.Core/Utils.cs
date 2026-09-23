using System.Reflection;

namespace Crimson.Core;

/// <summary>
/// Core utilities.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Get the informational version of an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get the version of.</param>
    /// <returns>The informational version of the assembly.</returns>
    public static string? GetVersionAttribute(this Assembly assembly)
        => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}