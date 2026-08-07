using System.Reflection;

namespace Crimson.Core;

/// <summary>
/// Contains utilities for loading embedded resources.
/// </summary>
public static class Resource
{
    /// <summary>
    /// Load an embedded resource from the given assembly.
    /// </summary>
    /// <param name="resourceName">The fully qualified resource name to load.</param>
    /// <param name="assembly">The assembly to load the resource from.</param>
    /// <returns>A byte array containing the resource's data.</returns>
    public static byte[] Load(string resourceName, Assembly assembly)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Could not find the resource \"{resourceName}\" in assembly {assembly}");

        byte[] data = new byte[stream.Length];
        stream.ReadExactly(data.AsSpan());

        return data;
    }
}