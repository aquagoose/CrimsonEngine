file(GLOB CGE_SRC CONFIGURE_DEPENDS
        include/Crimson/Core/*.h
        src/Core/*.h
        src/Core/*.cpp
)

add_library(Crimson STATIC ${CGE_SRC})
target_include_directories(Crimson PUBLIC include)