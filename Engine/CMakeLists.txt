file(GLOB_RECURSE CGE_SRC CONFIGURE_DEPENDS
        Crimson/Core/*.h
        Crimson/Core/*.cpp
)

add_library(Crimson STATIC ${CGE_SRC})