#include "UnitTestFramework.h"

#include <Math/Vec4.h>

#define ASSERT_VEC4(vec, x, y, z, w) {\
    assert(vec.X == x);\
    assert(vec.Y == y);\
    assert(vec.Z == z);\
    assert(vec.W == w);\
}

#define ASSERT_VEC4_APPROX(vec, x, y, z, w) {\
    assert(APPROX_EQ(vec.X, x));\
    assert(APPROX_EQ(vec.Y, y));\
    assert(APPROX_EQ(vec.Z, z));\
    assert(APPROX_EQ(vec.W, w));\
}

using namespace cge;

int main(int argc, char* argv[])
{
    // construction
    {
        Vec4i vec(1, 2, 3, 4);
        ASSERT_VEC4(vec, 1, 2, 3, 4);
    }

    // vec3 construction
    {
        Vec4i vec(Vec3i(1, 2, 3), 4);
        ASSERT_VEC4(vec, 1, 2, 3, 4);
    }

    // scalar construction
    {
        Vec4i vec(1);
        ASSERT_VEC4(vec, 1, 1, 1, 1);
    }

    // as
    {
        Vec4f a = { 1.1f, 2.2f, 3.3f, 4.4f };
        Vec4i b = a.As<i32>();
        ASSERT_VEC4(b, 1, 2, 3, 4);
    }

    // addition
    {
        Vec4i a = { 1, 2, 3, 4 };
        Vec4i b = { 5, 6, 7, 8 };
        Vec4i c = a + b;
        ASSERT_VEC4(c, 1 + 5, 2 + 6, 3 + 7, 4 + 8);
    }

    // subtraction
    {
        Vec4i a = { 1, 2, 3, 4 };
        Vec4i b = { 5, 6, 7, 8 };
        Vec4i c = a - b;
        ASSERT_VEC4(c, 1 - 5, 2 - 6, 3 - 7, 4 - 8);
    }

    // multiplication
    {
        Vec4i a = { 1, 2, 3, 4 };
        Vec4i b = { 5, 6, 7, 8 };
        Vec4i c = a * b;
        ASSERT_VEC4(c, 1 * 5, 2 * 6, 3 * 7, 4 * 8);
    }

    // multiplication (scalar)
    {
        Vec4i a = { 1, 2, 3, 4 };
        i32 b = 2;
        Vec4 c = a * b;
        ASSERT_VEC4(c, 1 * 2, 2 * 2, 3 * 2, 4 * 2);
    }

    // division
    {
        Vec4f a = { 1, 2, 3, 4 };
        Vec4f b = { 5, 6, 7, 8 };
        Vec4f c = a / b;
        ASSERT_VEC4(c, 1.0f / 5.0f, 2.0f / 6.0f, 3.0f / 7.0f, 4.0f / 8.0f);
    }

    // division (scalar)
    {
        Vec4f a = { 1, 2, 3, 4 };
        f32 b = 2;
        Vec4f c = a / b;
        ASSERT_VEC4(c, 1.0f / 2.0f, 2.0f / 2.0f, 3.0f / 2.0f, 4.0f / 2.0f);
    }

    // unit x
    {
        Vec4i vec = Vec4i::UnitX();
        ASSERT_VEC4(vec, 1, 0, 0, 0);
    }

    // unit y
    {
        Vec4i vec = Vec4i::UnitY();
        ASSERT_VEC4(vec, 0, 1, 0, 0);
    }

    // unit z
    {
        Vec4i vec = Vec4i::UnitZ();
        ASSERT_VEC4(vec, 0, 0, 1, 0);
    }

    // unit w
    {
        Vec4i vec = Vec4i::UnitW();
        ASSERT_VEC4(vec, 0, 0, 0, 1);
    }

    // dot product
    {
        Vec4i a = { 1, 2, 3, 4 };
        Vec4i b = { 5, 6, 7, 8 };
        i32 c = Vec4i::Dot(a, b);
        // https://www.calculatorsoup.com/calculators/algebra/dot-product-calculator.php
        assert(c == 70);
    }

    // length squared
    {
        Vec4i vec = { 1, 2, 3, 4 };
        i32 lengthSquared = vec.LengthSquared();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(lengthSquared == 30);
    }

    // length
    {
        Vec4d vec = { 1, 2, 3, 4 };
        f64 length = vec.Length();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(APPROX_EQ(length, 5.47723));
    }

    // normalization
    {
        Vec4d vec { 1, 2, 3, 4 };
        Vec4d normalized = vec.Normalized();
        // https://www.redcrab-software.com/en/Calculator/Vector/Normalization
        ASSERT_VEC4_APPROX(normalized, 0.18, 0.37, 0.55, 0.73);
    }

    // lerp
    {
        Vec4d a(2);
        Vec4d b(4);
        Vec4d c = Vec4d::Lerp(a, b, 0.5);
        ASSERT_VEC4_APPROX(c, 3, 3, 3, 3);
    }

    return 0;
}