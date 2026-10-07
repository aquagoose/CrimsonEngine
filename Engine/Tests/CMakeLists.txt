file(GLOB_RECURSE CGE_TESTS CONFIGURE_DEPENDS *.cpp)

foreach (file IN LISTS CGE_TESTS)
    # get the file name, and its parent directory
    cmake_path(GET file STEM filename)
    cmake_path(GET file PARENT_PATH dir)
    # get the parent directory, relevant to the current directory (which is tests/)
    cmake_path(RELATIVE_PATH dir BASE_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} OUTPUT_VARIABLE reldir)
    # replace the path separator with an underscore.
    # doing it like this means we can get Test_Dir1_Dir2_TestName etc.
    # todo does this work on windows?
    string(REPLACE "/" "_" dirpath ${reldir})

    set(testname "Test_${dirpath}_${filename}")
    message(STATUS "${testname} -> ${file}")
    add_executable(${testname} WIN32 ${file})
    target_link_libraries(${testname} PRIVATE Crimson)
endforeach ()