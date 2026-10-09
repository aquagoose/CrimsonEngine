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

    extension(Console)
    {
        /// <summary>
        /// Write a u8 string to the console.
        /// </summary>
        /// <param name="u8str">The u8 string to write.</param>
        public static void WriteLine(ReadOnlySpan<byte> u8str)
        {
            foreach (byte b in u8str)
                Console.Write((char) b);
            
            Console.WriteLine();
        }
    }
}