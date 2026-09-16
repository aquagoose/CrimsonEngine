#include <metal_stdlib>
#include <metal_math>
#include <metal_texture>
using namespace metal;

#line 90 "core"
struct pixelOutput_0
{
    float4 output_0 [[color(0)]];
};


#line 90
struct pixelInput_0
{
    float2 TexCoord_0 [[user(TEXCOORD)]];
    float4 Tint_0 [[user(COLOR)]];
};


#line 90
struct Camera2D_default_0
{
    matrix<float,int(4),int(4)>  Projection_0;
};


#line 90
struct KernelContext_0
{
    Camera2D_default_0 constant* camera_0;
    texture2d<float, access::sample> sprite_texture_0;
    sampler sprite_sampler_0;
};


#line 40 "TextureBatcher.slang"
[[fragment]] pixelOutput_0 PSMain(pixelInput_0 _S1 [[stage_in]], float4 Position_0 [[position]], Camera2D_default_0 constant* camera_1 [[buffer(0)]], texture2d<float, access::sample> sprite_texture_1 [[texture(0)]], sampler sprite_sampler_1 [[sampler(0)]])
{

#line 40
    thread KernelContext_0 kernelContext_0;

#line 40
    (&kernelContext_0)->camera_0 = camera_1;

#line 40
    (&kernelContext_0)->sprite_texture_0 = sprite_texture_1;

#line 40
    (&kernelContext_0)->sprite_sampler_0 = sprite_sampler_1;

    ;

#line 42
    pixelOutput_0 _S2 = { (((&kernelContext_0)->sprite_texture_0).sample(((&kernelContext_0)->sprite_sampler_0), (_S1.TexCoord_0))) * _S1.Tint_0 };

#line 42
    return _S2;
}


#line 42
struct VSMain_Result_0
{
    float4 Position_1 [[position]];
    float2 TexCoord_1 [[user(TEXCOORD)]];
    float4 Tint_1 [[user(COLOR)]];
};


#line 42
struct vertexInput_0
{
    float2 Position_2 [[attribute(0)]];
    float2 TexCoord_2 [[attribute(1)]];
    float4 Tint_2 [[attribute(2)]];
};


#line 10
struct VSOutput_0
{
    float4 Position_3;
    float2 TexCoord_3;
    float4 Tint_3;
};


#line 10
[[vertex]] VSMain_Result_0 VSMain(vertexInput_0 _S3 [[stage_in]], Camera2D_default_0 constant* camera_2 [[buffer(0)]], texture2d<float, access::sample> sprite_texture_2 [[texture(0)]], sampler sprite_sampler_2 [[sampler(0)]])
{

#line 10
    thread KernelContext_0 kernelContext_1;

#line 10
    (&kernelContext_1)->camera_0 = camera_2;

#line 10
    (&kernelContext_1)->sprite_texture_0 = sprite_texture_2;

#line 10
    (&kernelContext_1)->sprite_sampler_0 = sprite_sampler_2;

#line 29
    thread VSOutput_0 output_1;


    (&output_1)->Position_3 = (((float4(_S3.Position_2, 0.0f, 1.0f)) * (camera_2->Projection_0)));
    (&output_1)->TexCoord_3 = _S3.TexCoord_2;
    (&output_1)->Tint_3 = _S3.Tint_2;

#line 34
    thread VSMain_Result_0 _S4;

#line 34
    (&_S4)->Position_1 = output_1.Position_3;

#line 34
    (&_S4)->TexCoord_1 = output_1.TexCoord_3;

#line 34
    (&_S4)->Tint_1 = output_1.Tint_3;

#line 34
    return _S4;
}

