cmake_minimum_required(VERSION 3.12)
project(Grid9 VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Include directories
include_directories(include)

# Grid9 library
add_library(grid9 STATIC
    src/UniformPrecisionCoordinateCompressor.cpp
    src/CoordinateOperations.cpp
)

target_include_directories(grid9 PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)

# Demo executable
add_executable(grid9_demo
    demo/demo.cpp
)

target_link_libraries(grid9_demo grid9)

# Enable testing
enable_testing()

# Find Google Test (optional)
find_package(GTest QUIET)

if(GTest_FOUND)
    add_executable(grid9_test
        test/test_uniform_precision.cpp
        test/test_coordinate_operations.cpp
    )
    
    target_link_libraries(grid9_test grid9 gtest gtest_main)
    
    add_test(NAME Grid9Tests COMMAND grid9_test)
else()
    message(STATUS "Google Test not found. Tests will not be built.")
    message(STATUS "To install Google Test on Ubuntu: sudo apt-get install libgtest-dev")
    message(STATUS "To install Google Test with vcpkg: vcpkg install gtest")
endif()

# Installation
install(TARGETS grid9
    EXPORT Grid9Targets
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
    RUNTIME DESTINATION bin
    INCLUDES DESTINATION include
)

install(DIRECTORY include/ DESTINATION include)

install(EXPORT Grid9Targets
    FILE Grid9Targets.cmake
    NAMESPACE Grid9::
    DESTINATION lib/cmake/Grid9
)

# Create package config
include(CMakePackageConfigHelpers)
configure_package_config_file(
    "${CMAKE_CURRENT_SOURCE_DIR}/cmake/Grid9Config.cmake.in"
    "${CMAKE_CURRENT_BINARY_DIR}/Grid9Config.cmake"
    INSTALL_DESTINATION lib/cmake/Grid9
)

write_basic_package_version_file(
    "${CMAKE_CURRENT_BINARY_DIR}/Grid9ConfigVersion.cmake"
    VERSION ${PROJECT_VERSION}
    COMPATIBILITY AnyNewerVersion
)

install(FILES
    "${CMAKE_CURRENT_BINARY_DIR}/Grid9Config.cmake"
    "${CMAKE_CURRENT_BINARY_DIR}/Grid9ConfigVersion.cmake"
    DESTINATION lib/cmake/Grid9
)