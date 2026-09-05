option(CGE_SUBSYSTEM_GRAPHICS "Enable the graphics subsystem" ON)

# core engine sources
file(GLOB_RECURSE CGE_SRC CONFIGURE_DEPENDS
        Core/*.h
        Core/*.cpp
        Math/*.h
        Math/*.cpp
)

set(CGE_LIBS "")

if (CGE_SUBSYSTEM_GRAPHICS)
    file(GLOB_RECURSE CGE_GRAPHICS_SRC CONFIGURE_DEPENDS Graphics/*.h Graphics/*.cpp)
    list(APPEND CGE_SRC ${CGE_GRAPHICS_SRC})
    add_compile_definitions(CGE_SUBSYSTEM_GRAPHICS)
    list(APPEND CGE_LIBS SDL3::SDL3 SDL3_shadercross::SDL3_shadercross stb)
endif ()

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC .)
target_link_libraries(Crimson PUBLIC ${CGE_LIBS})

if (WIN32)
    add_compile_definitions(CGE_PLATFORM_WINDOWS)
elseif (UNIX)
    add_compile_definitions(CGE_PLATFORM_UNIX)
    if (APPLE)
        add_compile_definitions(CGE_PLATFORM_APPLE)
    elseif (LINUX)
        add_compile_definitions(CGE_PLATFORM_LINUX)
    endif ()
endif ()