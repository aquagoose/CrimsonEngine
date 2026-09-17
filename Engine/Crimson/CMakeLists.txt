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
    list(APPEND CGE_LIBS SDL3::SDL3 stb SDL3_shadercross::SDL3_shadercross)
endif ()

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC .)
target_link_libraries(Crimson PUBLIC ${CGE_LIBS})

if (WIN32)
    target_compile_definitions(Crimson PUBLIC CGE_PLATFORM_WINDOWS)
elseif (UNIX)
    target_compile_definitions(Crimson PUBLIC CGE_PLATFORM_UNIX)
    if (APPLE)
        target_compile_definitions(Crimson PUBLIC CGE_PLATFORM_APPLE)
    elseif (LINUX)
        target_compile_definitions(Crimson PUBLIC CGE_PLATFORM_LINUX)
    endif ()
endif ()

function (cge_setup_content target content_name content_dir)
    get_target_property(CRIMSON_DIR Crimson SOURCE_DIR)
    get_filename_component(ENGINE_DIR ${CRIMSON_DIR} DIRECTORY)

    # TODO: this copies ALL files but we only want to copy the .spv files. also should compile the files from scratch.
    add_custom_command(TARGET ${target} POST_BUILD
        COMMAND "${CMAKE_COMMAND}" -E copy_directory "${ENGINE_DIR}/Shaders" "$<TARGET_FILE_DIR:${target}>/${content_name}/Shaders"
        VERBATIM
    )

    target_compile_definitions(Crimson PUBLIC CGE_CONTENT_DIR="${content_name}")
endfunction()