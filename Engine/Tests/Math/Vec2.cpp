#include "UnitTestFramework.h"

#include <Math/Vec2.h>

#define ASSERT_VEC2(vec, x, y) {\
    assert(vec.X == x);\
    assert(vec.Y == y);\
}

#define ASSERT_VEC2_APPROX(vec, x, y) {\
    assert(APPROX_EQ(vec.X, x));\
    assert(APPROX_EQ(vec.Y, y));\
}

using namespace cge;

int main(int argc, char* argv[])
{
    // construction
    {
        Vec2i vec(1, 2);
        ASSERT_VEC2(vec, 1, 2);
    }

    // scalar construction
    {
        Vec2i vec(1);
        ASSERT_VEC2(vec, 1, 1);
    }

    // as
    {
        Vec2f a = { 1.1f, 2.2f };
        Vec2i b = a.As<i32>();
        ASSERT_VEC2(b, 1, 2);
    }

    // addition
    {
        Vec2i a = { 1, 2 };
        Vec2i b = { 5, 6 };
        Vec2i c = a + b;
        ASSERT_VEC2(c, 1 + 5, 2 + 6);
    }

    // subtraction
    {
        Vec2i a = { 1, 2 };
        Vec2i b = { 5, 6 };
        Vec2i c = a - b;
        ASSERT_VEC2(c, 1 - 5, 2 - 6);
    }

    // multiplication
    {
        Vec2i a = { 1, 2 };
        Vec2i b = { 5, 6 };
        Vec2i c = a * b;
        ASSERT_VEC2(c, 1 * 5, 2 * 6);
    }

    // multiplication (scalar)
    {
        Vec2i a = { 1, 2 };
        i32 b = 2;
        Vec2 c = a * b;
        ASSERT_VEC2(c, 1 * 2, 2 * 2);
    }

    // division
    {
        Vec2f a = { 1, 2 };
        Vec2f b = { 5, 6 };
        Vec2f c = a / b;
        ASSERT_VEC2(c, 1.0f / 5.0f, 2.0f / 6.0f);
    }

    // division (scalar)
    {
        Vec2f a = { 1, 2 };
        f32 b = 2;
        Vec2f c = a / b;
        ASSERT_VEC2(c, 1.0f / 2.0f, 2.0f / 2.0f);
    }

    // unit x
    {
        Vec2i vec = Vec2i::UnitX();
        ASSERT_VEC2(vec, 1, 0);
    }

    // unit y
    {
        Vec2i vec = Vec2i::UnitY();
        ASSERT_VEC2(vec, 0, 1);
    }

    // dot product
    {
        Vec2i a = { 1, 2 };
        Vec2i b = { 5, 6 };
        i32 c = Vec2i::Dot(a, b);
        // https://www.calculatorsoup.com/calculators/algebra/dot-product-calculator.php
        assert(c == 17);
    }

    // length squared
    {
        Vec2i vec = { 1, 2 };
        i32 lengthSquared = vec.LengthSquared();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(lengthSquared == 5);
    }

    // length
    {
        Vec2d vec = { 1, 2 };
        f64 length = vec.Length();
        // https://calculator-online.net/vector-magnitude-calculator/
        assert(APPROX_EQ(length, 2.23607));
    }

    // normalization
    {
        Vec2d vec { 1, 2 };
        Vec2d normalized = vec.Normalized();
        // https://www.redcrab-software.com/en/Calculator/Vector/Normalization
        ASSERT_VEC2_APPROX(normalized, 0.45, 0.89);
    }

    // lerp
    {
        Vec2d a(2);
        Vec2d b(4);
        Vec2d c = Vec2d::Lerp(a, b, 0.5);
        ASSERT_VEC2_APPROX(c, 3, 3);
    }

    return 0;
}