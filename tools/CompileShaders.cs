#!/usr/bin/env dotnet

using System.Diagnostics;

const string VSMain = "VSMain";
const string PSMain = "PSMain";

Console.WriteLine("Crimson Shader Compiler");
Console.WriteLine("=======================");

if (args.Length < 1)
{
    Console.WriteLine("Must provide a directory to compile!");
    return 1;
}

string dir = Path.GetFullPath(args[0]);
string[] files = Directory.GetFiles(dir, "*.hlsl", SearchOption.AllDirectories);

foreach (string file in files)
{
    Console.WriteLine(file);
    string hlsl = File.ReadAllText(file);
    string fileName = Path.GetFileNameWithoutExtension(file);
    string outFileDir = Path.GetDirectoryName(file);

    bool hasVSMain = hlsl.Contains(VSMain);
    bool hasPSMain = hlsl.Contains(PSMain);

    if (hasVSMain)
    {
        Console.WriteLine($"  Compiling {VSMain}...");
        string outFile = Path.Combine(outFileDir, $"{fileName}_v.spv");
        if (!CompileShader(file, outFile, "vs_6_0", VSMain))
            return 1;
    }

    if (hasPSMain)
    {
        Console.WriteLine($"  Compiling {PSMain}...");
        string outFile = Path.Combine(outFileDir, $"{fileName}_p.spv");
        if (!CompileShader(file, outFile, "ps_6_0", PSMain))
            return 1;
    }
}

return 0;

bool CompileShader(string file, string outFile, string profile, string entryPoint)
{
    ProcessStartInfo startInfo = new ProcessStartInfo("dxc");
    startInfo.ArgumentList.Add("-spirv");
    startInfo.ArgumentList.Add("-Fo");
    startInfo.ArgumentList.Add(outFile);
    startInfo.ArgumentList.Add("-T");
    startInfo.ArgumentList.Add(profile);
    startInfo.ArgumentList.Add("-E");
    startInfo.ArgumentList.Add(entryPoint);
    startInfo.ArgumentList.Add(file);

    Process dxc = new Process()
    {
        StartInfo = startInfo
    };

    dxc.Start();
    dxc.WaitForExit();

    return dxc.ExitCode == 0;
}