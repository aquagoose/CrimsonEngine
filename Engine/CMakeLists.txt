option(CGE_BUILD_TESTS "Build the Crimson Engine tests" ON)

file(GLOB CGE_SRC CONFIGURE_DEPENDS
        include/Crimson/Core/*.h
        src/Core/*.h
        src/Core/*.cpp
)

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC include)

if (CGE_BUILD_TESTS)
    message(STATUS "Searching for tests")
    list(APPEND CMAKE_MESSAGE_INDENT "  ")
    file(GLOB TEST_DIRS tests/*)
    foreach (dir IN LISTS TEST_DIRS)
        get_filename_component(dirname ${dir} NAME_WE)
        file(GLOB TEST_SRC ${dir}/*.cpp)
        foreach (file IN LISTS TEST_SRC)
            get_filename_component(filename ${file} NAME_WE)
            set(testname "Test_${dirname}_${filename}")
            message(STATUS "${testname} -> ${file}")
            add_executable(${testname} ${file})
            target_link_libraries(${testname} PRIVATE Crimson)
        endforeach ()
    endforeach ()
    list(POP_BACK CMAKE_MESSAGE_INDENT)
endif ()