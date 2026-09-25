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