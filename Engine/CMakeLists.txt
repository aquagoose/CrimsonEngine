file(GLOB_RECURSE CGE_SRC CONFIGURE_DEPENDS
        Crimson/Core/*.h
        Crimson/Core/*.cpp
)

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC Crimson)

add_subdirectory(Tests)