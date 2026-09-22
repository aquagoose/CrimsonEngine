using System.Reflection;

namespace Crimson.Core;

public static class Resource
{
    public static byte[] Load(string name, Assembly assembly)
    {
        using Stream? stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            throw new FileNotFoundException($"Could not load resource \"{name}\" from assembly {assembly}.");

        byte[] result = new byte[stream.Length];
        stream.ReadExactly(result);

        return result;
    }
}