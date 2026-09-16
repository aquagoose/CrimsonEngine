#pragma once

#include "Math/Coredefs.h"

namespace cge
{
    /**
     * A floating-point RGBA color.
     */
    struct Color
    {
        f32 R;
        f32 G;
        f32 B;
        f32 A;

        Color(float r, float g, float b, float a = 1.0f) : R(r), G(g), B(b), A(a) {}

        Color(u8 r, u8 g, u8 b, u8 a = U8_MAX) :
            R(static_cast<f32>(r) / static_cast<f32>(U8_MAX)),
            G(static_cast<f32>(g) / static_cast<f32>(U8_MAX)),
            B(static_cast<f32>(b) / static_cast<f32>(U8_MAX)),
            A(static_cast<f32>(a) / static_cast<f32>(U8_MAX))
        {}

        /**
         * AliceBlue has an RGB value of 240, 248, 255 (0xF0F8FF)
         */
        static Color AliceBlue() { return { 0.9411764705882353f, 0.9725490196078431f, 1.0f, 1.0f }; }

        /**
         * AntiqueWhite has an RGB value of 250, 235, 215 (0xFAEBD7)
         */
        static Color AntiqueWhite() { return { 0.9803921568627451f, 0.9215686274509803f, 0.8431372549019608f, 1.0f }; }

        /**
         * Aqua has an RGB value of 0, 255, 255 (0x00FFFF)
         */
        static Color Aqua() { return { 0.0f, 1.0f, 1.0f, 1.0f }; }

        /**
         * Aquamarine has an RGB value of 127, 255, 212 (0x7FFFD4)
         */
        static Color Aquamarine() { return { 0.4980392156862745f, 1.0f, 0.8313725490196079f, 1.0f }; }

        /**
         * Azure has an RGB value of 240, 255, 255 (0xF0FFFF)
         */
        static Color Azure() { return { 0.9411764705882353f, 1.0f, 1.0f, 1.0f }; }

        /**
         * Beige has an RGB value of 245, 245, 220 (0xF5F5DC)
         */
        static Color Beige() { return { 0.9607843137254902f, 0.9607843137254902f, 0.8627450980392157f, 1.0f }; }

        /**
         * Bisque has an RGB value of 255, 228, 196 (0xFFE4C4)
         */
        static Color Bisque() { return { 1.0f, 0.8941176470588236f, 0.7686274509803922f, 1.0f }; }

        /**
         * Black has an RGB value of 0, 0, 0 (0x000000)
         */
        static Color Black() { return { 0.0f, 0.0f, 0.0f, 1.0f }; }

        /**
         * BlanchedAlmond has an RGB value of 255, 235, 205 (0xFFEBCD)
         */
        static Color BlanchedAlmond() { return { 1.0f, 0.9215686274509803f, 0.803921568627451f, 1.0f }; }

        /**
         * Blue has an RGB value of 0, 0, 255 (0x0000FF)
         */
        static Color Blue() { return { 0.0f, 0.0f, 1.0f, 1.0f }; }

        /**
         * BlueViolet has an RGB value of 138, 43, 226 (0x8A2BE2)
         */
        static Color BlueViolet() { return { 0.5411764705882353f, 0.16862745098039217f, 0.8862745098039215f, 1.0f }; }

        /**
         * Brown has an RGB value of 165, 42, 42 (0xA52A2A)
         */
        static Color Brown() { return { 0.6470588235294118f, 0.16470588235294117f, 0.16470588235294117f, 1.0f }; }

        /**
         * BurlyWood has an RGB value of 222, 184, 135 (0xDEB887)
         */
        static Color BurlyWood() { return { 0.8705882352941177f, 0.7215686274509804f, 0.5294117647058824f, 1.0f }; }

        /**
         * CadetBlue has an RGB value of 95, 158, 160 (0x5F9EA0)
         */
        static Color CadetBlue() { return { 0.37254901960784315f, 0.6196078431372549f, 0.6274509803921569f, 1.0f }; }

        /**
         * Chartreuse has an RGB value of 127, 255, 0 (0x7FFF00)
         */
        static Color Chartreuse() { return { 0.4980392156862745f, 1.0f, 0.0f, 1.0f }; }

        /**
         * Chocolate has an RGB value of 210, 105, 30 (0xD2691E)
         */
        static Color Chocolate() { return { 0.8235294117647058f, 0.4117647058823529f, 0.11764705882352941f, 1.0f }; }

        /**
         * Coral has an RGB value of 255, 127, 80 (0xFF7F50)
         */
        static Color Coral() { return { 1.0f, 0.4980392156862745f, 0.3137254901960784f, 1.0f }; }

        /**
         * CornflowerBlue has an RGB value of 100, 149, 237 (0x6495ED)
         */
        static Color CornflowerBlue() { return { 0.39215686274509803f, 0.5843137254901961f, 0.9294117647058824f, 1.0f }; }

        /**
         * Cornsilk has an RGB value of 255, 248, 220 (0xFFF8DC)
         */
        static Color Cornsilk() { return { 1.0f, 0.9725490196078431f, 0.8627450980392157f, 1.0f }; }

        /**
         * Crimson has an RGB value of 220, 20, 60 (0xDC143C)
         */
        static Color Crimson() { return { 0.8627450980392157f, 0.0784313725490196f, 0.23529411764705882f, 1.0f }; }

        /**
         * Cyan has an RGB value of 0, 255, 255 (0x00FFFF)
         */
        static Color Cyan() { return { 0.0f, 1.0f, 1.0f, 1.0f }; }

        /**
         * DarkBlue has an RGB value of 0, 0, 139 (0x00008B)
         */
        static Color DarkBlue() { return { 0.0f, 0.0f, 0.5450980392156862f, 1.0f }; }

        /**
         * DarkCyan has an RGB value of 0, 139, 139 (0x008B8B)
         */
        static Color DarkCyan() { return { 0.0f, 0.5450980392156862f, 0.5450980392156862f, 1.0f }; }

        /**
         * DarkGoldenRod has an RGB value of 184, 134, 11 (0xB8860B)
         */
        static Color DarkGoldenRod() { return { 0.7215686274509804f, 0.5254901960784314f, 0.043137254901960784f, 1.0f }; }

        /**
         * DarkGray has an RGB value of 169, 169, 169 (0xA9A9A9)
         */
        static Color DarkGray() { return { 0.6627450980392157f, 0.6627450980392157f, 0.6627450980392157f, 1.0f }; }

        /**
         * DarkGrey has an RGB value of 169, 169, 169 (0xA9A9A9)
         */
        static Color DarkGrey() { return { 0.6627450980392157f, 0.6627450980392157f, 0.6627450980392157f, 1.0f }; }

        /**
         * DarkGreen has an RGB value of 0, 100, 0 (0x006400)
         */
        static Color DarkGreen() { return { 0.0f, 0.39215686274509803f, 0.0f, 1.0f }; }

        /**
         * DarkKhaki has an RGB value of 189, 183, 107 (0xBDB76B)
         */
        static Color DarkKhaki() { return { 0.7411764705882353f, 0.7176470588235294f, 0.4196078431372549f, 1.0f }; }

        /**
         * DarkMagenta has an RGB value of 139, 0, 139 (0x8B008B)
         */
        static Color DarkMagenta() { return { 0.5450980392156862f, 0.0f, 0.5450980392156862f, 1.0f }; }

        /**
         * DarkOliveGreen has an RGB value of 85, 107, 47 (0x556B2F)
         */
        static Color DarkOliveGreen() { return { 0.3333333333333333f, 0.4196078431372549f, 0.1843137254901961f, 1.0f }; }

        /**
         * DarkOrange has an RGB value of 255, 140, 0 (0xFF8C00)
         */
        static Color DarkOrange() { return { 1.0f, 0.5490196078431373f, 0.0f, 1.0f }; }

        /**
         * DarkOrchid has an RGB value of 153, 50, 204 (0x9932CC)
         */
        static Color DarkOrchid() { return { 0.6f, 0.19607843137254902f, 0.8f, 1.0f }; }

        /**
         * DarkRed has an RGB value of 139, 0, 0 (0x8B0000)
         */
        static Color DarkRed() { return { 0.5450980392156862f, 0.0f, 0.0f, 1.0f }; }

        /**
         * DarkSalmon has an RGB value of 233, 150, 122 (0xE9967A)
         */
        static Color DarkSalmon() { return { 0.9137254901960784f, 0.5882352941176471f, 0.47843137254901963f, 1.0f }; }

        /**
         * DarkSeaGreen has an RGB value of 143, 188, 143 (0x8FBC8F)
         */
        static Color DarkSeaGreen() { return { 0.5607843137254902f, 0.7372549019607844f, 0.5607843137254902f, 1.0f }; }

        /**
         * DarkSlateBlue has an RGB value of 72, 61, 139 (0x483D8B)
         */
        static Color DarkSlateBlue() { return { 0.2823529411764706f, 0.23921568627450981f, 0.5450980392156862f, 1.0f }; }

        /**
         * DarkSlateGray has an RGB value of 47, 79, 79 (0x2F4F4F)
         */
        static Color DarkSlateGray() { return { 0.1843137254901961f, 0.30980392156862746f, 0.30980392156862746f, 1.0f }; }

        /**
         * DarkSlateGrey has an RGB value of 47, 79, 79 (0x2F4F4F)
         */
        static Color DarkSlateGrey() { return { 0.1843137254901961f, 0.30980392156862746f, 0.30980392156862746f, 1.0f }; }

        /**
         * DarkTurquoise has an RGB value of 0, 206, 209 (0x00CED1)
         */
        static Color DarkTurquoise() { return { 0.0f, 0.807843137254902f, 0.8196078431372549f, 1.0f }; }

        /**
         * DarkViolet has an RGB value of 148, 0, 211 (0x9400D3)
         */
        static Color DarkViolet() { return { 0.5803921568627451f, 0.0f, 0.8274509803921568f, 1.0f }; }

        /**
         * DeepPink has an RGB value of 255, 20, 147 (0xFF1493)
         */
        static Color DeepPink() { return { 1.0f, 0.0784313725490196f, 0.5764705882352941f, 1.0f }; }

        /**
         * DeepSkyBlue has an RGB value of 0, 191, 255 (0x00BFFF)
         */
        static Color DeepSkyBlue() { return { 0.0f, 0.7490196078431373f, 1.0f, 1.0f }; }

        /**
         * DimGray has an RGB value of 105, 105, 105 (0x696969)
         */
        static Color DimGray() { return { 0.4117647058823529f, 0.4117647058823529f, 0.4117647058823529f, 1.0f }; }

        /**
         * DimGrey has an RGB value of 105, 105, 105 (0x696969)
         */
        static Color DimGrey() { return { 0.4117647058823529f, 0.4117647058823529f, 0.4117647058823529f, 1.0f }; }

        /**
         * DodgerBlue has an RGB value of 30, 144, 255 (0x1E90FF)
         */
        static Color DodgerBlue() { return { 0.11764705882352941f, 0.5647058823529412f, 1.0f, 1.0f }; }

        /**
         * FireBrick has an RGB value of 178, 34, 34 (0xB22222)
         */
        static Color FireBrick() { return { 0.6980392156862745f, 0.13333333333333333f, 0.13333333333333333f, 1.0f }; }

        /**
         * FloralWhite has an RGB value of 255, 250, 240 (0xFFFAF0)
         */
        static Color FloralWhite() { return { 1.0f, 0.9803921568627451f, 0.9411764705882353f, 1.0f }; }

        /**
         * ForestGreen has an RGB value of 34, 139, 34 (0x228B22)
         */
        static Color ForestGreen() { return { 0.13333333333333333f, 0.5450980392156862f, 0.13333333333333333f, 1.0f }; }

        /**
         * Fuchsia has an RGB value of 255, 0, 255 (0xFF00FF)
         */
        static Color Fuchsia() { return { 1.0f, 0.0f, 1.0f, 1.0f }; }

        /**
         * Gainsboro has an RGB value of 220, 220, 220 (0xDCDCDC)
         */
        static Color Gainsboro() { return { 0.8627450980392157f, 0.8627450980392157f, 0.8627450980392157f, 1.0f }; }

        /**
         * GhostWhite has an RGB value of 248, 248, 255 (0xF8F8FF)
         */
        static Color GhostWhite() { return { 0.9725490196078431f, 0.9725490196078431f, 1.0f, 1.0f }; }

        /**
         * Gold has an RGB value of 255, 215, 0 (0xFFD700)
         */
        static Color Gold() { return { 1.0f, 0.8431372549019608f, 0.0f, 1.0f }; }

        /**
         * GoldenRod has an RGB value of 218, 165, 32 (0xDAA520)
         */
        static Color GoldenRod() { return { 0.8549019607843137f, 0.6470588235294118f, 0.12549019607843137f, 1.0f }; }

        /**
         * Gray has an RGB value of 128, 128, 128 (0x808080)
         */
        static Color Gray() { return { 0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f, 1.0f }; }

        /**
         * Grey has an RGB value of 128, 128, 128 (0x808080)
         */
        static Color Grey() { return { 0.5019607843137255f, 0.5019607843137255f, 0.5019607843137255f, 1.0f }; }

        /**
         * Green has an RGB value of 0, 128, 0 (0x008000)
         */
        static Color Green() { return { 0.0f, 0.5019607843137255f, 0.0f, 1.0f }; }

        /**
         * GreenYellow has an RGB value of 173, 255, 47 (0xADFF2F)
         */
        static Color GreenYellow() { return { 0.6784313725490196f, 1.0f, 0.1843137254901961f, 1.0f }; }

        /**
         * HoneyDew has an RGB value of 240, 255, 240 (0xF0FFF0)
         */
        static Color HoneyDew() { return { 0.9411764705882353f, 1.0f, 0.9411764705882353f, 1.0f }; }

        /**
         * HotPink has an RGB value of 255, 105, 180 (0xFF69B4)
         */
        static Color HotPink() { return { 1.0f, 0.4117647058823529f, 0.7058823529411765f, 1.0f }; }

        /**
         * IndianRed has an RGB value of 205, 92, 92 (0xCD5C5C)
         */
        static Color IndianRed() { return { 0.803921568627451f, 0.3607843137254902f, 0.3607843137254902f, 1.0f }; }

        /**
         * Indigo has an RGB value of 75, 0, 130 (0x4B0082)
         */
        static Color Indigo() { return { 0.29411764705882354f, 0.0f, 0.5098039215686274f, 1.0f }; }

        /**
         * Ivory has an RGB value of 255, 255, 240 (0xFFFFF0)
         */
        static Color Ivory() { return { 1.0f, 1.0f, 0.9411764705882353f, 1.0f }; }

        /**
         * Khaki has an RGB value of 240, 230, 140 (0xF0E68C)
         */
        static Color Khaki() { return { 0.9411764705882353f, 0.9019607843137255f, 0.5490196078431373f, 1.0f }; }

        /**
         * Lavender has an RGB value of 230, 230, 250 (0xE6E6FA)
         */
        static Color Lavender() { return { 0.9019607843137255f, 0.9019607843137255f, 0.9803921568627451f, 1.0f }; }

        /**
         * LavenderBlush has an RGB value of 255, 240, 245 (0xFFF0F5)
         */
        static Color LavenderBlush() { return { 1.0f, 0.9411764705882353f, 0.9607843137254902f, 1.0f }; }

        /**
         * LawnGreen has an RGB value of 124, 252, 0 (0x7CFC00)
         */
        static Color LawnGreen() { return { 0.48627450980392156f, 0.9882352941176471f, 0.0f, 1.0f }; }

        /**
         * LemonChiffon has an RGB value of 255, 250, 205 (0xFFFACD)
         */
        static Color LemonChiffon() { return { 1.0f, 0.9803921568627451f, 0.803921568627451f, 1.0f }; }

        /**
         * LightBlue has an RGB value of 173, 216, 230 (0xADD8E6)
         */
        static Color LightBlue() { return { 0.6784313725490196f, 0.8470588235294118f, 0.9019607843137255f, 1.0f }; }

        /**
         * LightCoral has an RGB value of 240, 128, 128 (0xF08080)
         */
        static Color LightCoral() { return { 0.9411764705882353f, 0.5019607843137255f, 0.5019607843137255f, 1.0f }; }

        /**
         * LightCyan has an RGB value of 224, 255, 255 (0xE0FFFF)
         */
        static Color LightCyan() { return { 0.8784313725490196f, 1.0f, 1.0f, 1.0f }; }

        /**
         * LightGoldenRodYellow has an RGB value of 250, 250, 210 (0xFAFAD2)
         */
        static Color LightGoldenRodYellow() { return { 0.9803921568627451f, 0.9803921568627451f, 0.8235294117647058f, 1.0f }; }

        /**
         * LightGray has an RGB value of 211, 211, 211 (0xD3D3D3)
         */
        static Color LightGray() { return { 0.8274509803921568f, 0.8274509803921568f, 0.8274509803921568f, 1.0f }; }

        /**
         * LightGrey has an RGB value of 211, 211, 211 (0xD3D3D3)
         */
        static Color LightGrey() { return { 0.8274509803921568f, 0.8274509803921568f, 0.8274509803921568f, 1.0f }; }

        /**
         * LightGreen has an RGB value of 144, 238, 144 (0x90EE90)
         */
        static Color LightGreen() { return { 0.5647058823529412f, 0.9333333333333333f, 0.5647058823529412f, 1.0f }; }

        /**
         * LightPink has an RGB value of 255, 182, 193 (0xFFB6C1)
         */
        static Color LightPink() { return { 1.0f, 0.7137254901960784f, 0.7568627450980392f, 1.0f }; }

        /**
         * LightSalmon has an RGB value of 255, 160, 122 (0xFFA07A)
         */
        static Color LightSalmon() { return { 1.0f, 0.6274509803921569f, 0.47843137254901963f, 1.0f }; }

        /**
         * LightSeaGreen has an RGB value of 32, 178, 170 (0x20B2AA)
         */
        static Color LightSeaGreen() { return { 0.12549019607843137f, 0.6980392156862745f, 0.6666666666666666f, 1.0f }; }

        /**
         * LightSkyBlue has an RGB value of 135, 206, 250 (0x87CEFA)
         */
        static Color LightSkyBlue() { return { 0.5294117647058824f, 0.807843137254902f, 0.9803921568627451f, 1.0f }; }

        /**
         * LightSlateGray has an RGB value of 119, 136, 153 (0x778899)
         */
        static Color LightSlateGray() { return { 0.4666666666666667f, 0.5333333333333333f, 0.6f, 1.0f }; }

        /**
         * LightSlateGrey has an RGB value of 119, 136, 153 (0x778899)
         */
        static Color LightSlateGrey() { return { 0.4666666666666667f, 0.5333333333333333f, 0.6f, 1.0f }; }

        /**
         * LightSteelBlue has an RGB value of 176, 196, 222 (0xB0C4DE)
         */
        static Color LightSteelBlue() { return { 0.6901960784313725f, 0.7686274509803922f, 0.8705882352941177f, 1.0f }; }

        /**
         * LightYellow has an RGB value of 255, 255, 224 (0xFFFFE0)
         */
        static Color LightYellow() { return { 1.0f, 1.0f, 0.8784313725490196f, 1.0f }; }

        /**
         * Lime has an RGB value of 0, 255, 0 (0x00FF00)
         */
        static Color Lime() { return { 0.0f, 1.0f, 0.0f, 1.0f }; }

        /**
         * LimeGreen has an RGB value of 50, 205, 50 (0x32CD32)
         */
        static Color LimeGreen() { return { 0.19607843137254902f, 0.803921568627451f, 0.19607843137254902f, 1.0f }; }

        /**
         * Linen has an RGB value of 250, 240, 230 (0xFAF0E6)
         */
        static Color Linen() { return { 0.9803921568627451f, 0.9411764705882353f, 0.9019607843137255f, 1.0f }; }

        /**
         * Magenta has an RGB value of 255, 0, 255 (0xFF00FF)
         */
        static Color Magenta() { return { 1.0f, 0.0f, 1.0f, 1.0f }; }

        /**
         * Maroon has an RGB value of 128, 0, 0 (0x800000)
         */
        static Color Maroon() { return { 0.5019607843137255f, 0.0f, 0.0f, 1.0f }; }

        /**
         * MediumAquaMarine has an RGB value of 102, 205, 170 (0x66CDAA)
         */
        static Color MediumAquaMarine() { return { 0.4f, 0.803921568627451f, 0.6666666666666666f, 1.0f }; }

        /**
         * MediumBlue has an RGB value of 0, 0, 205 (0x0000CD)
         */
        static Color MediumBlue() { return { 0.0f, 0.0f, 0.803921568627451f, 1.0f }; }

        /**
         * MediumOrchid has an RGB value of 186, 85, 211 (0xBA55D3)
         */
        static Color MediumOrchid() { return { 0.7294117647058823f, 0.3333333333333333f, 0.8274509803921568f, 1.0f }; }

        /**
         * MediumPurple has an RGB value of 147, 112, 219 (0x9370DB)
         */
        static Color MediumPurple() { return { 0.5764705882352941f, 0.4392156862745098f, 0.8588235294117647f, 1.0f }; }

        /**
         * MediumSeaGreen has an RGB value of 60, 179, 113 (0x3CB371)
         */
        static Color MediumSeaGreen() { return { 0.23529411764705882f, 0.7019607843137254f, 0.44313725490196076f, 1.0f }; }

        /**
         * MediumSlateBlue has an RGB value of 123, 104, 238 (0x7B68EE)
         */
        static Color MediumSlateBlue() { return { 0.4823529411764706f, 0.40784313725490196f, 0.9333333333333333f, 1.0f }; }

        /**
         * MediumSpringGreen has an RGB value of 0, 250, 154 (0x00FA9A)
         */
        static Color MediumSpringGreen() { return { 0.0f, 0.9803921568627451f, 0.6039215686274509f, 1.0f }; }

        /**
         * MediumTurquoise has an RGB value of 72, 209, 204 (0x48D1CC)
         */
        static Color MediumTurquoise() { return { 0.2823529411764706f, 0.8196078431372549f, 0.8f, 1.0f }; }

        /**
         * MediumVioletRed has an RGB value of 199, 21, 133 (0xC71585)
         */
        static Color MediumVioletRed() { return { 0.7803921568627451f, 0.08235294117647059f, 0.5215686274509804f, 1.0f }; }

        /**
         * MidnightBlue has an RGB value of 25, 25, 112 (0x191970)
         */
        static Color MidnightBlue() { return { 0.09803921568627451f, 0.09803921568627451f, 0.4392156862745098f, 1.0f }; }

        /**
         * MintCream has an RGB value of 245, 255, 250 (0xF5FFFA)
         */
        static Color MintCream() { return { 0.9607843137254902f, 1.0f, 0.9803921568627451f, 1.0f }; }

        /**
         * MistyRose has an RGB value of 255, 228, 225 (0xFFE4E1)
         */
        static Color MistyRose() { return { 1.0f, 0.8941176470588236f, 0.8823529411764706f, 1.0f }; }

        /**
         * Moccasin has an RGB value of 255, 228, 181 (0xFFE4B5)
         */
        static Color Moccasin() { return { 1.0f, 0.8941176470588236f, 0.7098039215686275f, 1.0f }; }

        /**
         * NavajoWhite has an RGB value of 255, 222, 173 (0xFFDEAD)
         */
        static Color NavajoWhite() { return { 1.0f, 0.8705882352941177f, 0.6784313725490196f, 1.0f }; }

        /**
         * Navy has an RGB value of 0, 0, 128 (0x000080)
         */
        static Color Navy() { return { 0.0f, 0.0f, 0.5019607843137255f, 1.0f }; }

        /**
         * OldLace has an RGB value of 253, 245, 230 (0xFDF5E6)
         */
        static Color OldLace() { return { 0.9921568627450981f, 0.9607843137254902f, 0.9019607843137255f, 1.0f }; }

        /**
         * Olive has an RGB value of 128, 128, 0 (0x808000)
         */
        static Color Olive() { return { 0.5019607843137255f, 0.5019607843137255f, 0.0f, 1.0f }; }

        /**
         * OliveDrab has an RGB value of 107, 142, 35 (0x6B8E23)
         */
        static Color OliveDrab() { return { 0.4196078431372549f, 0.5568627450980392f, 0.13725490196078433f, 1.0f }; }

        /**
         * Orange has an RGB value of 255, 165, 0 (0xFFA500)
         */
        static Color Orange() { return { 1.0f, 0.6470588235294118f, 0.0f, 1.0f }; }

        /**
         * OrangeRed has an RGB value of 255, 69, 0 (0xFF4500)
         */
        static Color OrangeRed() { return { 1.0f, 0.27058823529411763f, 0.0f, 1.0f }; }

        /**
         * Orchid has an RGB value of 218, 112, 214 (0xDA70D6)
         */
        static Color Orchid() { return { 0.8549019607843137f, 0.4392156862745098f, 0.8392156862745098f, 1.0f }; }

        /**
         * PaleGoldenRod has an RGB value of 238, 232, 170 (0xEEE8AA)
         */
        static Color PaleGoldenRod() { return { 0.9333333333333333f, 0.9098039215686274f, 0.6666666666666666f, 1.0f }; }

        /**
         * PaleGreen has an RGB value of 152, 251, 152 (0x98FB98)
         */
        static Color PaleGreen() { return { 0.596078431372549f, 0.984313725490196f, 0.596078431372549f, 1.0f }; }

        /**
         * PaleTurquoise has an RGB value of 175, 238, 238 (0xAFEEEE)
         */
        static Color PaleTurquoise() { return { 0.6862745098039216f, 0.9333333333333333f, 0.9333333333333333f, 1.0f }; }

        /**
         * PaleVioletRed has an RGB value of 219, 112, 147 (0xDB7093)
         */
        static Color PaleVioletRed() { return { 0.8588235294117647f, 0.4392156862745098f, 0.5764705882352941f, 1.0f }; }

        /**
         * PapayaWhip has an RGB value of 255, 239, 213 (0xFFEFD5)
         */
        static Color PapayaWhip() { return { 1.0f, 0.9372549019607843f, 0.8352941176470589f, 1.0f }; }

        /**
         * PeachPuff has an RGB value of 255, 218, 185 (0xFFDAB9)
         */
        static Color PeachPuff() { return { 1.0f, 0.8549019607843137f, 0.7254901960784313f, 1.0f }; }

        /**
         * Peru has an RGB value of 205, 133, 63 (0xCD853F)
         */
        static Color Peru() { return { 0.803921568627451f, 0.5215686274509804f, 0.24705882352941178f, 1.0f }; }

        /**
         * Pink has an RGB value of 255, 192, 203 (0xFFC0CB)
         */
        static Color Pink() { return { 1.0f, 0.7529411764705882f, 0.796078431372549f, 1.0f }; }

        /**
         * Plum has an RGB value of 221, 160, 221 (0xDDA0DD)
         */
        static Color Plum() { return { 0.8666666666666667f, 0.6274509803921569f, 0.8666666666666667f, 1.0f }; }

        /**
         * PowderBlue has an RGB value of 176, 224, 230 (0xB0E0E6)
         */
        static Color PowderBlue() { return { 0.6901960784313725f, 0.8784313725490196f, 0.9019607843137255f, 1.0f }; }

        /**
         * Purple has an RGB value of 128, 0, 128 (0x800080)
         */
        static Color Purple() { return { 0.5019607843137255f, 0.0f, 0.5019607843137255f, 1.0f }; }

        /**
         * RebeccaPurple has an RGB value of 102, 51, 153 (0x663399)
         */
        static Color RebeccaPurple() { return { 0.4f, 0.2f, 0.6f, 1.0f }; }

        /**
         * Red has an RGB value of 255, 0, 0 (0xFF0000)
         */
        static Color Red() { return { 1.0f, 0.0f, 0.0f, 1.0f }; }

        /**
         * RosyBrown has an RGB value of 188, 143, 143 (0xBC8F8F)
         */
        static Color RosyBrown() { return { 0.7372549019607844f, 0.5607843137254902f, 0.5607843137254902f, 1.0f }; }

        /**
         * RoyalBlue has an RGB value of 65, 105, 225 (0x4169E1)
         */
        static Color RoyalBlue() { return { 0.2549019607843137f, 0.4117647058823529f, 0.8823529411764706f, 1.0f }; }

        /**
         * SaddleBrown has an RGB value of 139, 69, 19 (0x8B4513)
         */
        static Color SaddleBrown() { return { 0.5450980392156862f, 0.27058823529411763f, 0.07450980392156863f, 1.0f }; }

        /**
         * Salmon has an RGB value of 250, 128, 114 (0xFA8072)
         */
        static Color Salmon() { return { 0.9803921568627451f, 0.5019607843137255f, 0.4470588235294118f, 1.0f }; }

        /**
         * SandyBrown has an RGB value of 244, 164, 96 (0xF4A460)
         */
        static Color SandyBrown() { return { 0.9568627450980393f, 0.6431372549019608f, 0.3764705882352941f, 1.0f }; }

        /**
         * SeaGreen has an RGB value of 46, 139, 87 (0x2E8B57)
         */
        static Color SeaGreen() { return { 0.1803921568627451f, 0.5450980392156862f, 0.3411764705882353f, 1.0f }; }

        /**
         * SeaShell has an RGB value of 255, 245, 238 (0xFFF5EE)
         */
        static Color SeaShell() { return { 1.0f, 0.9607843137254902f, 0.9333333333333333f, 1.0f }; }

        /**
         * Sienna has an RGB value of 160, 82, 45 (0xA0522D)
         */
        static Color Sienna() { return { 0.6274509803921569f, 0.3215686274509804f, 0.17647058823529413f, 1.0f }; }

        /**
         * Silver has an RGB value of 192, 192, 192 (0xC0C0C0)
         */
        static Color Silver() { return { 0.7529411764705882f, 0.7529411764705882f, 0.7529411764705882f, 1.0f }; }

        /**
         * SkyBlue has an RGB value of 135, 206, 235 (0x87CEEB)
         */
        static Color SkyBlue() { return { 0.5294117647058824f, 0.807843137254902f, 0.9215686274509803f, 1.0f }; }

        /**
         * SlateBlue has an RGB value of 106, 90, 205 (0x6A5ACD)
         */
        static Color SlateBlue() { return { 0.41568627450980394f, 0.35294117647058826f, 0.803921568627451f, 1.0f }; }

        /**
         * SlateGray has an RGB value of 112, 128, 144 (0x708090)
         */
        static Color SlateGray() { return { 0.4392156862745098f, 0.5019607843137255f, 0.5647058823529412f, 1.0f }; }

        /**
         * SlateGrey has an RGB value of 112, 128, 144 (0x708090)
         */
        static Color SlateGrey() { return { 0.4392156862745098f, 0.5019607843137255f, 0.5647058823529412f, 1.0f }; }

        /**
         * Snow has an RGB value of 255, 250, 250 (0xFFFAFA)
         */
        static Color Snow() { return { 1.0f, 0.9803921568627451f, 0.9803921568627451f, 1.0f }; }

        /**
         * SpringGreen has an RGB value of 0, 255, 127 (0x00FF7F)
         */
        static Color SpringGreen() { return { 0.0f, 1.0f, 0.4980392156862745f, 1.0f }; }

        /**
         * SteelBlue has an RGB value of 70, 130, 180 (0x4682B4)
         */
        static Color SteelBlue() { return { 0.27450980392156865f, 0.5098039215686274f, 0.7058823529411765f, 1.0f }; }

        /**
         * Tan has an RGB value of 210, 180, 140 (0xD2B48C)
         */
        static Color Tan() { return { 0.8235294117647058f, 0.7058823529411765f, 0.5490196078431373f, 1.0f }; }

        /**
         * Teal has an RGB value of 0, 128, 128 (0x008080)
         */
        static Color Teal() { return { 0.0f, 0.5019607843137255f, 0.5019607843137255f, 1.0f }; }

        /**
         * Thistle has an RGB value of 216, 191, 216 (0xD8BFD8)
         */
        static Color Thistle() { return { 0.8470588235294118f, 0.7490196078431373f, 0.8470588235294118f, 1.0f }; }

        /**
         * Tomato has an RGB value of 255, 99, 71 (0xFF6347)
         */
        static Color Tomato() { return { 1.0f, 0.38823529411764707f, 0.2784313725490196f, 1.0f }; }

        /**
         * Turquoise has an RGB value of 64, 224, 208 (0x40E0D0)
         */
        static Color Turquoise() { return { 0.25098039215686274f, 0.8784313725490196f, 0.8156862745098039f, 1.0f }; }

        /**
         * Violet has an RGB value of 238, 130, 238 (0xEE82EE)
         */
        static Color Violet() { return { 0.9333333333333333f, 0.5098039215686274f, 0.9333333333333333f, 1.0f }; }

        /**
         * Wheat has an RGB value of 245, 222, 179 (0xF5DEB3)
         */
        static Color Wheat() { return { 0.9607843137254902f, 0.8705882352941177f, 0.7019607843137254f, 1.0f }; }

        /**
         * White has an RGB value of 255, 255, 255 (0xFFFFFF)
         */
        static Color White() { return { 1.0f, 1.0f, 1.0f, 1.0f }; }

        /**
         * WhiteSmoke has an RGB value of 245, 245, 245 (0xF5F5F5)
         */
        static Color WhiteSmoke() { return { 0.9607843137254902f, 0.9607843137254902f, 0.9607843137254902f, 1.0f }; }

        /**
         * Yellow has an RGB value of 255, 255, 0 (0xFFFF00)
         */
        static Color Yellow() { return { 1.0f, 1.0f, 0.0f, 1.0f }; }

        /**
         * YellowGreen has an RGB value of 154, 205, 50 (0x9ACD32)
         */
        static Color YellowGreen() { return { 0.6039215686274509f, 0.803921568627451f, 0.19607843137254902f, 1.0f }; }
    };
}
