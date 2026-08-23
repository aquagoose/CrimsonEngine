add_library(Crimson INTERFACE)

add_subdirectory(Core)
target_link_libraries(Crimson INTERFACE Crimson-Core)