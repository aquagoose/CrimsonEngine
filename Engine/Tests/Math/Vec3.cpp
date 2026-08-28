#include "UnitTestFramework.h"

#include <Math/Vec3.h>

#define ASSERT_VEC3(vec, x, y, z) {\
    assert(vec.X == x);\
    assert(vec.Y == y);\
    assert(vec.Z == z);\
}

#define ASSERT_VEC3_APPROX(vec, x, y, z) {\
    assert(APPROX_EQ(vec.X, x));\
    assert(APPROX_EQ(vec.Y, y));\
    assert(APPROX_EQ(vec.Z, z));\
}

using namespace cge;

int main(int argc, char* argv[])
{
    // construction
    {
        Vec3i vec(1, 2, 3);
        ASSERT_VEC3(vec, 1, 2, 3);
    }

    // scalar construction
    {
        Vec3i vec(1);
        ASSERT_VEC3(vec, 1, 1, 1);
    }

    // as
    {
        Vec3f a = { 1.1f, 2.2f, 3.3f };
        Vec3i b = a.As<i32>();
        ASSERT_VEC3(b, 1, 2, 3);
    }

    // addition
    {
        Vec3i a = { 1, 2, 3 };
        Vec3i b = { 5, 6, 7 };
        Vec3i c = a + b;
        ASSERT_VEC3(c, 1 + 5, 2 + 6, 3 + 7);
    }

    // subtraction
    {
        Vec3i a = { 1, 2, 3 };
        Vec3i b = { 5, 6, 7 };
        Vec3i c = a - b;
        ASSERT_VEC3(c, 1 - 5, 2 - 6, 3 - 7);
    }

    // multiplication
    {
        Vec3i a = { 1, 2, 3 };
        Vec3i b = { 5, 6, 7 };
        Vec3i c = a * b;
        ASSERT_VEC3(c, 1 * 5, 2 * 6, 3 * 7);
    }

    // multiplication (scalar)
    {
        Vec3i a = { 1, 2, 3 };
        i32 b = 2;
        Vec3 c = a * b;
        ASSERT_VEC3(c, 1 * 2, 2 * 2, 3 * 2);
    }

    // division
    {
        Vec3f a = { 1, 2, 3 };
        Vec3f b = { 5, 6, 7 };
        Vec3f c = a / b;
        ASSERT_VEC3(c, 1.0f / 5.0f, 2.0f / 6.0f, 3.0f / 7.0f);
    }

    // division (scalar)
    {
        Vec3f a = { 1, 2, 3 };
        f32 b = 2;
        Vec3f c = a / b;
        ASSERT_VEC3(c, 1.0f / 2.0f, 2.0f / 2.0f, 3.0f / 2.0f);
    }

    // unit x
    {
        Vec3i vec = Vec3i::UnitX();
        ASSERT_VEC3(vec, 1, 0, 0);
    }

    // unit y
    {
        Vec3i vec = Vec3i::UnitY();
        ASSERT_VEC3(vec, 0, 1, 0);
    }

    // unit z
    {
        Vec3i vec = Vec3i::UnitZ();
        ASSERT_VEC3(vec, 0, 0, 1);
    }

    // dot product
    {
        Vec3i a = { 1, 2, 3 };
        Vec3i b = { 5, 6, 7 };
        i32 c = Vec3i::Dot(a, b);
        // https://www.calculatorsoup.com/calculators/algebra/dot-product-calculator.php
        assert(c == 38);
    }

    // length squared
    {
        Vec3i vec = { 1, 2, 3 };
        i32 lengthSquared = vec.LengthSquared();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(lengthSquared == 14);
    }

    // length
    {
        Vec3d vec = { 1, 2, 3 };
        f64 length = vec.Length();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(APPROX_EQ(length, 3.74166));
    }

    // normalization
    {
        Vec3d vec { 1, 2, 3 };
        Vec3d normalized = vec.Normalized();
        // https://www.redcrab-software.com/en/Calculator/Vector/Normalization
        ASSERT_VEC3_APPROX(normalized, 0.27, 0.53, 0.80);
    }

    // lerp
    {
        Vec3d a(2);
        Vec3d b(4);
        Vec3d c = Vec3d::Lerp(a, b, 0.5);
        ASSERT_VEC3_APPROX(c, 3, 3, 3);
    }

    return 0;
}