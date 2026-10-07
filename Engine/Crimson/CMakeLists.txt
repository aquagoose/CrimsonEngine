option(CGE_SUBSYSTEM_GRAPHICS "Enable the graphics subsystem" ON)

file(GLOB_RECURSE CGE_SRC CONFIGURE_DEPENDS
        # the following libraries are not optional and are always added
        Core/*.h
        Core/*.cpp
)

if (CGE_SUBSYSTEM_GRAPHICS)
    file(GLOB_RECURSE CGE_GRAPHICS_SRC CONFIGURE_DEPENDS
            Graphics/*.h
            Graphics/*.cpp
    )
    list(APPEND CGE_SRC ${CGE_GRAPHICS_SRC})
endif ()

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})