option(CGE_ENABLE_GRAPHICS "Enable the graphics subsystem" ON)

set(CGE_LINKLIBS "")

file(GLOB CGE_SRC CONFIGURE_DEPENDS
        include/Crimson/*
        src/*

        # always include core in the src
        include/Crimson/Core/*
        src/Core/*
)

if (CGE_ENABLE_GRAPHICS)
    file(GLOB_RECURSE CGE_GRAPHICS_SRC CONFIGURE_DEPENDS
            include/Crimson/Graphics/*
            src/Graphics/*
    )
    list(APPEND CGE_SRC ${CGE_GRAPHICS_SRC})
    list(APPEND CGE_LINKLIBS SDL3::SDL3)
endif ()

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC include)
target_link_libraries(Crimson PUBLIC ${CGE_LINKLIBS})

message(STATUS "Searching for tests")
list(APPEND CMAKE_MESSAGE_INDENT "  ")
file(GLOB_RECURSE CGE_TEST_SRC tests/*.cpp)
foreach (file IN LISTS CGE_TEST_SRC)
    cmake_path(GET file PARENT_PATH dir)
    cmake_path(GET dir FILENAME dirname)
    cmake_path(GET file STEM filename)
    set(testname "Test_${dirname}_${filename}")
    message(STATUS "${testname} -> ${file}")
    add_executable(${testname} WIN32 ${file})
    target_link_libraries(${testname} PRIVATE Crimson)
endforeach ()
list(POP_BACK CMAKE_MESSAGE_INDENT)