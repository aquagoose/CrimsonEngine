add_library(Crimson INTERFACE)

add_subdirectory(Core)
target_link_libraries(Crimson INTERFACE Crimson-Core)

add_subdirectory(Graphics)
target_link_libraries(Crimson INTERFACE Crimson-Graphics)