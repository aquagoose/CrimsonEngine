using System.Runtime.CompilerServices;
using piko.Core;
using piko.SDL3;

namespace Crimson.Graphics.Utils;

internal static class SDLUtils
{
    public static SDL.GPUColorTargetBlendState NoBlend => new SDL.GPUColorTargetBlendState
    {
        EnableBlend = false
    };
    
    public static SDL.GPUColorTargetBlendState NonPremultipliedBlend => new SDL.GPUColorTargetBlendState
    {
        EnableBlend = true,
        SrcColorBlendfactor = SDL.GPUBlendFactor.SrcAlpha,
        DstColorBlendfactor = SDL.GPUBlendFactor.OneMinusSrcAlpha,
        ColorBlendOp = SDL.GPUBlendOp.Add,
        SrcAlphaBlendfactor = SDL.GPUBlendFactor.SrcAlpha,
        DstAlphaBlendfactor = SDL.GPUBlendFactor.OneMinusSrcAlpha,
        AlphaBlendOp = SDL.GPUBlendOp.Add
    };
    
    public static void Check(this bool b, string operation)
    {
        if (!b)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");
    }

    public static nint Check(this nint value, string operation)
    {
        if (value == 0)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");

        return value;
    }

    public static T Check<T>(this T handle, string operation) where T : IHandle
    {
        if (handle.IsNull)
            throw new Exception($"SDL operation \"{operation}\" failed: {SDL.GetError()}");

        return handle;
    }

    public static SDL.FColor ToFColor(this Color color)
        => new SDL.FColor(color.R, color.G, color.B, color.A);

    public static SDL.GPUTextureFormat ToSDL(this PixelFormat format)
    {
        return format switch
        {
            PixelFormat.RGBA8 => SDL.GPUTextureFormat.R8g8b8a8Unorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public static SDL.GPUFilter ToSDLFilter(this Filter filter)
    {
        return filter switch
        {
            Filter.Linear => SDL.GPUFilter.Linear,
            Filter.Nearest => SDL.GPUFilter.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }

    public static SDL.GPUSamplerMipmapMode ToSDLMipmapMode(this Filter filter)
    {
        return filter switch
        {
            Filter.Linear => SDL.GPUSamplerMipmapMode.Linear,
            Filter.Nearest => SDL.GPUSamplerMipmapMode.Nearest,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }

    public static SDL.GPUSamplerAddressMode ToSDL(this AddressMode mode)
    {
        return mode switch
        {
            AddressMode.Repeat => SDL.GPUSamplerAddressMode.Repeat,
            AddressMode.Clamp => SDL.GPUSamplerAddressMode.ClampToEdge,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public static uint CalculateNumMips(uint width, uint height)
        => (uint) double.Floor(double.Log2(double.Max(width, height))) + 1;

    extension(SDL)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void BindGPUVertexBuffer(SDL.GPURenderPass renderPass, uint slot, SDL.GPUBuffer buffer,
            uint offset = 0)
        {
            SDL.GPUBufferBinding binding = new()
            {
                Buffer = buffer,
                Offset = offset
            };
            
            SDL.BindGPUVertexBuffers(renderPass, slot, &binding, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void BindGPUIndexBuffer(SDL.GPURenderPass renderPass, SDL.GPUBuffer buffer,
            SDL.GPUIndexElementSize elementSize, uint offset = 0)
        {
            SDL.GPUBufferBinding binding = new()
            {
                Buffer = buffer,
                Offset = offset
            };
            
            SDL.BindGPUIndexBuffer(renderPass, &binding, elementSize);
        }

        // todo sampler should be integrated into texture
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void BindGPUFragmentTextures(SDL.GPURenderPass pass, uint firstSlot, ReadOnlySpan<Texture> textures)
        {
            SDL.GPUTextureSamplerBinding* bindings = stackalloc SDL.GPUTextureSamplerBinding[textures.Length];
            for (int i = 0; i < textures.Length; i++)
                bindings[i] = new SDL.GPUTextureSamplerBinding(textures[i].TextureHandle, textures[i].SamplerHandle);
            
            SDL.BindGPUFragmentSamplers(pass, firstSlot, bindings, (uint) textures.Length);
        }
    }
}