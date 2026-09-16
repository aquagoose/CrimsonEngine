#include <iostream>

#include "UnitTestFramework.h"

#include <Math/Matrix.h>

using namespace cge;

int main(int argc, char* argv[])
{
    {
        Matrixi a
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        };

        Matrixi b
        {
            16, 15, 14, 13,
            12, 11, 10, 9,
            8, 7, 6, 5,
            4, 3, 2, 1
        };

        Matrixi c = a * b;

        assert(c.Row0.X == 80);
        assert(c.Row0.Y == 70);
        assert(c.Row0.Z == 60);
        assert(c.Row0.W == 50);
        assert(c.Row1.X == 240);
        assert(c.Row1.Y == 214);
        assert(c.Row1.Z == 188);
        assert(c.Row1.W == 162);
        assert(c.Row2.X == 400);
        assert(c.Row2.Y == 358);
        assert(c.Row2.Z == 316);
        assert(c.Row2.W == 274);
        assert(c.Row3.X == 560);
        assert(c.Row3.Y == 502);
        assert(c.Row3.Z == 444);
        assert(c.Row3.W == 386);
    }
}