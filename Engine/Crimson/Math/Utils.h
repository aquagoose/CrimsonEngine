#pragma once

#include <cmath>

#define CGE_TORAD(degrees) (degrees * (M_PI / 180))
#define CGE_TODEG(radians) (radians * (180 / M_PI))

#define CGE_LERP(from, to, amount) (amount * (to - from) + from)