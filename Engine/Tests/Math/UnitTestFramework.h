#pragma once

#include <cassert>

#define APPROX_EQ(value, equals) value <= (equals + 0.01f) && value >= (equals - 0.01f)