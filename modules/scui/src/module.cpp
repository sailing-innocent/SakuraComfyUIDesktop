/**
 * @file module.cpp
 * @brief The Module Impl
 * @author sailing-innocent
 * @date 2025-07-11
 */

#include "SkrCore/module/module.hpp"
#include "SkrCore/log.h"
#include "SCUI/hello.h"
#include "hv/TcpServer.h"
#include "libipc/shm.h"
#include "libipc/ipc.h"
#include <chrono>

using namespace std::chrono_literals;

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

    std::vector<char const*> const datas = {
        "hello!",
        "foo",
        "bar",
        "ISO/IEC",
        "14882:2011",
        "ISO/IEC 14882:2017 Information technology - Programming languages - C++",
        "ISO/IEC 14882:2020",
        "Modern C++ Design: Generic Programming and Design Patterns Applied"
    };

    // create a shared memory with name "my-ipc-channel"
    auto shm_id = ipc::shm::acquire("my-ipc-channel", 1024 * 1024, ipc::shm::create);
    if (!shm_id)
    {
        SKR_LOG_ERROR(u8"Failed to acquire shared memory!");
        return -1;
    }

    // dump the datas into the shared memory
    auto shm_mem = ipc::shm::get_mem(shm_id, nullptr);
    if (!shm_mem)
    {
        SKR_LOG_ERROR(u8"Failed to get shared memory!");
        ipc::shm::release(shm_id);
        return -1;
    }

    // dump the datas into the shared memory
    for (const auto& data : datas)
    {
        // COPY
        size_t data_size = strlen(data) + 1; // +1 for null terminator
        if (data_size > 1024 * 1024)         // Check if data size
        {
            SKR_LOG_ERROR(u8"Data size exceeds shared memory limit!");
            ipc::shm::release(shm_id);
            return -1;
        }
        memcpy(static_cast<char*>(shm_mem), data, data_size);
        shm_mem = static_cast<char*>(shm_mem) + data_size; // Move pointer
    }

    int port = 1234;
    hv::TcpServer server;
    int listenfd = server.createsocket(port);
    if (listenfd < 0)
    {
        SKR_LOG_ERROR(u8"Failed to create socket on port {}: {}", port, listenfd);
        return -1;
    }
    // event
    server.onConnection = [](const hv::SocketChannelPtr& channel) {
        auto peeraddr = channel->peeraddr();
        if (channel->isConnected())
        {
            SKR_LOG_INFO(u8"New connection from {%s}:{}", peeraddr.c_str());
        }
        else
        {
            SKR_LOG_INFO(u8"Connection closed from {%s}:{}", peeraddr.c_str());
        }
    };

    server.onMessage = [](const hv::SocketChannelPtr& channel, hv::Buffer* buf) {
        channel->write(buf);
    };
    server.setThreadNum(4);
    server.start();

    while (getchar() != 'q')
    {
        // Wait for user input to quit
    }

    return 0;
}

void SSCUIRuntimeModule::on_unload()
{
    SKR_LOG_INFO(u8"SCUIRuntime module unloaded!");
}
