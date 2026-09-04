#pragma once

#include "Logger.h"

#define CGE_CHECK_NULL(value) if (!value) CGE_FATAL("{} was null.", #value)