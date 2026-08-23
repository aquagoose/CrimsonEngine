file(GLOB_RECURSE CGE_GRAPHICS_SRC CONFIGURE_DEPENDS *.h *.cpp)
add_library(Crimson-Graphics STATIC ${CGE_GRAPHICS_SRC})
target_include_directories(Crimson-Graphics PUBLIC ../)

target_link_libraries(Crimson-Graphics PRIVATE Crimson-Core)
target_link_libraries(Crimson-Graphics PUBLIC SDL3::SDL3)