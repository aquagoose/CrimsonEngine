using System.Runtime.InteropServices;
using Crimson.Core;
using piko.SDL3;
using piko.SDL3.ShaderCross;

namespace Crimson.Graphics.Utils;

internal static class ShaderUtils
{
    /*public static void LoadGraphicsShader(Device device, string name, out ShaderModule? vertex, out ShaderModule? pixel)
    {
        Logger.Trace($"Compiling shader '{name}'.");
        
        vertex = null;
        pixel = null;
        
        string hlsl = File.ReadAllText(Path.Combine("Shaders", $"{name}.hlsl"));

        string? vertexEntryPoint = null;
        string? pixelEntryPoint = null;

        int i = 0;
        while ((i = hlsl.IndexOf("#pragma", i)) >= 0)
        {
            int j = hlsl.IndexOf('\n', i);

            string line = hlsl[i..j];

            // Yes i know this is inefficient but it works so I don't care
            string[] splitLine = line.Split(' ');

            switch (splitLine[1])
            {
                case "vertex":
                    vertexEntryPoint = splitLine[2];
                    break;
                case "pixel":
                    pixelEntryPoint = splitLine[2];
                    break;
            }

            i = j;
        }

        if (vertexEntryPoint != null)
        {
            Logger.Trace("    Compiling vertex...");
            byte[] spirv = Compiler.CompileHlsl(ShaderStage.Vertex, hlsl, vertexEntryPoint);
            vertex = device.CreateShaderModule(ShaderStage.Vertex, spirv, vertexEntryPoint);
        }

        if (pixelEntryPoint != null)
        {
            Logger.Trace("    Compiling pixel...");
            byte[] spirv = Compiler.CompileHlsl(ShaderStage.Pixel, hlsl, pixelEntryPoint);
            pixel = device.CreateShaderModule(ShaderStage.Pixel, spirv, pixelEntryPoint);
        }
    }*/
    
    public static void LoadGraphicsShader(SDL.GPUDevice device, string name, out SDL.GPUShader? vertexShader, out SDL.GPUShader? pixelShader)
    {
        Logger.Trace($"Compiling shader '{name}'.");

        vertexShader = null;
        pixelShader = null;

        string fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Content", "Shaders", $"{name}.hlsl"));
        string includeDir = Path.GetDirectoryName(fullPath);
        string hlsl = File.ReadAllText(fullPath);
        
        string? vertexEntryPoint = null;
        string? pixelEntryPoint = null;

        uint vertexUniforms = 0;
        uint pixelUniforms = 0;
        uint vertexSamplers = 0;
        uint pixelSamplers = 0;

        int i = 0;
        while ((i = hlsl.IndexOf("#pragma", i)) >= 0)
        {
            int j = hlsl.IndexOf('\n', i);

            string line = hlsl[i..j];

            // Yes i know this is inefficient but it works so I don't care
            string[] splitLine = line.Split(' ');

            switch (splitLine[1])
            {
                case "vertex":
                    vertexEntryPoint = splitLine[2];
                    vertexUniforms = uint.Parse(splitLine[3]);
                    vertexSamplers = uint.Parse(splitLine[4]);
                    break;
                case "pixel":
                    pixelEntryPoint = splitLine[2];
                    pixelUniforms = uint.Parse(splitLine[3]);
                    pixelSamplers = uint.Parse(splitLine[4]);
                    break;
            }

            i = j;
        }
        
        SDL.GPUShaderFormat shaderFormat = SDL.GetGPUShaderFormats(device);

        if (vertexEntryPoint != null)
        {
            Logger.Trace("Creating vertex shader.");
            vertexShader = CreateShader(device, SDL.GPUShaderStage.Vertex, shaderFormat, hlsl, vertexEntryPoint,
                includeDir, true, vertexUniforms, vertexSamplers);
        }

        if (pixelEntryPoint != null)
        {
            Logger.Trace("Creating pixel shader.");
            pixelShader = CreateShader(device, SDL.GPUShaderStage.Fragment, shaderFormat, hlsl, pixelEntryPoint,
                includeDir, true, pixelUniforms, pixelSamplers);
        }
    }

    private static unsafe SDL.GPUShader CreateShader(SDL.GPUDevice device, SDL.GPUShaderStage stage, SDL.GPUShaderFormat shaderFormat,
        string hlsl, string entryPoint, string? includeDir, bool debug, uint numUniforms, uint numSamplers)
    {
        sbyte* pHlsl = (sbyte*) Marshal.StringToHGlobalAnsi(hlsl);
        sbyte* pEntryPoint = (sbyte*) Marshal.StringToHGlobalAnsi(entryPoint);
        sbyte* pIncludeDir = (sbyte*) Marshal.StringToHGlobalAnsi(includeDir);

        SDLShaderCross.HLSLInfo hlslInfo = new()
        {
            ShaderStage = (SDLShaderCross.ShaderStage) stage,
            Source = pHlsl,
            Entrypoint = pEntryPoint,
            IncludeDir = pIncludeDir
        };
        
        SDL.GPUShaderCreateInfo shaderInfo = new()
        {
            Stage = stage,
            Format = shaderFormat,
            Entrypoint = pEntryPoint,
            NumSamplers = numSamplers,
            NumUniformBuffers = numUniforms
        };
        
        if ((shaderFormat & SDL.GPUShaderFormat.Dxil) != 0)
        {
            nuint dxilSize;
            void* dxil = SDLShaderCross.CompileDXILFromHLSL(&hlslInfo, &dxilSize);
            if (dxil == null)
                throw new Exception($"Failed to compile shader: {SDL.GetError()}");
            
            shaderInfo.Code = (byte*) dxil;
            shaderInfo.CodeSize = dxilSize;
            shaderInfo.Format = SDL.GPUShaderFormat.Dxil;
        }
        else if ((shaderFormat & SDL.GPUShaderFormat.Dxbc) != 0)
        {
            nuint dxbcSize;
            void* dxbc = SDLShaderCross.CompileDXBCFromHLSL(&hlslInfo, &dxbcSize);
            if (dxbc == null)
                throw new Exception($"Failed to compile shader: {SDL.GetError()}");
            
            shaderInfo.Code = (byte*) dxbc;
            shaderInfo.CodeSize = dxbcSize;
            shaderInfo.Format = SDL.GPUShaderFormat.Dxbc;
        }
        else
        {
            nuint spirvSize;
            void* spirv = SDLShaderCross.CompileSPIRVFromHLSL(&hlslInfo, &spirvSize);
            if (spirv == null)
                throw new Exception($"Failed to compile shader: {SDL.GetError()}");

            shaderInfo.Code = (byte*) spirv;
            shaderInfo.CodeSize = spirvSize;

            SDLShaderCross.SPIRVInfo spirvInfo = new()
            {
                ShaderStage = (SDLShaderCross.ShaderStage) stage,
                Bytecode = (byte*) spirv,
                BytecodeSize = spirvSize,
                Entrypoint = pEntryPoint
            };

            if ((shaderFormat & SDL.GPUShaderFormat.Msl) != 0)
            {
                void* msl = SDLShaderCross.TranspileMSLFromSPIRV(&spirvInfo);
                if (msl == null)
                    throw new Exception($"Failed to transpile SPIRV: {SDL.GetError()}");
                
                NativeMemory.Free(spirv);
                shaderInfo.Code = (byte*) msl;
                shaderInfo.CodeSize = strlen((sbyte*) msl);
                shaderInfo.Format = SDL.GPUShaderFormat.Msl;
            }
        }

        SDL.GPUShader shader = SDL.CreateGPUShader(device, &shaderInfo);
        NativeMemory.Free(shaderInfo.Code);
        
        if (shader.IsNull)
            throw new Exception($"Failed to create shader: {SDL.GetError()}");
        
        return shader;
    }

    /*public static unsafe IntPtr LoadGraphicsShader(IntPtr device, SDL.GPUShaderStage stage, string name, string entryPoint, uint numUniforms, uint numSamplers)
    {
        string basePath = Path.Combine("Shaders", $"{name}");

        string path = stage switch
        {
            SDL.GPUShaderStage.Vertex => basePath + "_v",
            SDL.GPUShaderStage.Fragment => basePath + "_p",
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };

        SDL.GPUShaderFormat shaderFormat = SDL.GetGPUShaderFormats(device);

        if (shaderFormat.HasFlag(SDL.GPUShaderFormat.DXBC))
            path += ".dxil";
        else if (shaderFormat.HasFlag(SDL.GPUShaderFormat.SPIRV))
            path += ".spv";
        else
            throw new NotSupportedException(shaderFormat.ToString());
        
        byte[] data = File.ReadAllBytes(path);

        fixed (byte* pData = data)
        {
            SDL.GPUShaderCreateInfo shaderInfo = new()
            {
                Stage = stage,
                Format = shaderFormat,
                Code = (nint) pData,
                CodeSize = (nuint) data.Length,
                NumUniformBuffers = numUniforms,
                NumSamplers = numSamplers,
                Entrypoint = Marshal.StringToCoTaskMemAnsi(entryPoint)
            };

            Logger.Trace("Creating shader.");
            return SDL.CreateGPUShader(device, in shaderInfo).Check("Create GPU shader");
        }
    }*/

    private static unsafe nuint strlen(sbyte* @string)
    {
        nuint length = 0;
        do
        {
            length++;
        } while (@string[length] != 0);

        return length;
    }
}