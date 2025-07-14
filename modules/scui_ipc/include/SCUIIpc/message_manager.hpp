#pragma once
/**
 * @file message_manager.hpp
 * @brief The Message Manager for IPC in SCUI
 * @author sailing-innocent
 * @date 2025-07-14
 */

#include "SCUIIpc/app_message.hpp"

namespace scui
{

struct SCUI_IPC_API IPCMessageManager {

public:
    static bool Initialize();
    static bool Finalize();
    static IPCMessageManager* Get();
};

} // namespace scui