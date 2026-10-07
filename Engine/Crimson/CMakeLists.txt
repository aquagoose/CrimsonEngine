file(GLOB_RECURSE CGE_SRC CONFIGURE_DEPENDS
        # the following libraries are not optional and are always added
        Core/*.h
        Core/*.cpp
)

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})