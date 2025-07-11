/**
 * @file module.cpp
 * @brief The Module Impl
 * @author sailing-innocent
 * @date 2025-07-11
 */

#include "SkrCore/module/module.hpp"
#include "SkrCore/log.h"
#include "SCUI/hello.h"

class SSCUIRuntimeModule : public skr::IDynamicModule
{
    virtual void on_load(int argc, char8_t** argv) override;
    virtual int main_module_exec(int argc, char8_t** argv) override;
    virtual void on_unload() override;
};

static SSCUIRuntimeModule* g_scui_app_module = nullptr;

IMPLEMENT_DYNAMIC_MODULE(SSCUIRuntimeModule, SCUIRuntime);

void SSCUIRuntimeModule::on_load(int argc, char8_t** argv)
{
    SKR_LOG_INFO(u8"SCUIRuntime module loaded!");
    scui::hello();
}

int SSCUIRuntimeModule::main_module_exec(int argc, char8_t** argv)
{
    SKR_LOG_INFO(u8"SCUIRuntime module executed as main module!");
    // Here you can initialize your SCUI application
    return 0;
}

void SSCUIRuntimeModule::on_unload()
{
    SKR_LOG_INFO(u8"SCUIRuntime module unloaded!");
}
