file(GLOB_RECURSE CGE_CORE_SRC *.h *.cpp)
add_library(Crimson-Core STATIC ${CGE_CORE_SRC})