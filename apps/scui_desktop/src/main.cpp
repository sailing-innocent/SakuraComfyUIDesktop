/**
 * @file main.cpp
 * @brief The Entry point for the SakuraComfyUIDesktop application.
 * @author sailing-innocent
 * @date 2025-07-11
 */

#include <iostream>
#include "SCUI/hello.h"
#include "SkrCore/module/module_manager.hpp"
#include "SkrOS/filesystem.hpp"
#include "SkrCore/log.h"

int main(int argc, char** argv)
{

    std::cout << "Welcome to SakuraComfyUIDesktop!!!" << std::endl;
    auto r = scui::hello();
    std::cout << "SCUI hello returned: " << r << std::endl;
    auto moduleManager = skr_get_module_manager();
    std::error_code ec = {};
    auto root = skr::filesystem::current_path(ec);
    moduleManager->mount(root.u8string().c_str());
    moduleManager->make_module_graph(u8"SCUIRuntime", true);
    auto result = moduleManager->init_module_graph(argc, argv);
    if (result != 0)
    {
        SKR_LOG_ERROR(u8"module graph init failed!");
    }
    moduleManager->destroy_module_graph();
    return 0;
}
