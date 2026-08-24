#include <Math/Vec4.h>
#include <cassert>

using namespace cge;

#define ASSERT_VEC4(vec, x, y, z, w) {\
    assert(vec.X == x);\
    assert(vec.Y == y);\
    assert(vec.Z == z);\
    assert(vec.W == w);\
}

int main(int argc, char* argv[])
{
    // construction
    {
        Vec4i vec(1, 2, 3, 4);
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

    return 0;
}