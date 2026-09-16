#pragma once

#include "Math/Matrix.h"
#include "Math/Vec3.h"

namespace cge
{
    struct Camera
    {
        Matrixf Projection;
        Matrixf View;
        Vec3f Position;
        f32 _padding1{};
    };
}