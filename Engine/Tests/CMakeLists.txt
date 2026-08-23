message(STATUS "Searching for tests.")
list(APPEND CMAKE_MESSAGE_INDENT "  ")

# get all test directories. structure is Test_<DirectoryName>_<TestName>
file(GLOB CGE_TEST_DIRS LIST_DIRECTORIES true *)

foreach (dir IN LISTS CGE_TEST_DIRS)
    if (NOT IS_DIRECTORY ${dir}) #ignore CMakeLists.txt file
        CONTINUE()
    endif ()
    get_filename_component(dirname ${dir} NAME_WE)
    file(GLOB_RECURSE CGE_TESTS CONFIGURE_DEPENDS ${dir}/*.cpp) # find all tests
    foreach (file IN LISTS CGE_TESTS)
        get_filename_component(filename ${file} NAME_WE)
        set(testname "Test_${dirname}_${filename}")
        message(STATUS "${testname}: ${file}")
        add_executable(${testname} ${file})
        target_link_libraries(${testname} Crimson)
    endforeach ()
endforeach ()

list(POP_BACK CMAKE_MESSAGE_INDENT)