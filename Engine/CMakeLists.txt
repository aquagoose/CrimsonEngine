file(GLOB CRIMSON_SRC CONFIGURE_DEPENDS
        include/Crimson/*.h
        src/*.h
        src/*.cpp

        # always include core in the src
        include/Crimson/Core/*.h
        src/Core/*.cpp
)

add_library(Crimson STATIC ${CRIMSON_SRC})
target_include_directories(Crimson PUBLIC include)

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