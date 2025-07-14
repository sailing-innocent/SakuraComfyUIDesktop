/**
 * @file message_manager.cpp
 * @brief The Message Manager Implementation for IPC in SCUI
 * @author sailing-innocent
 * @date 2025-07-14
 */

#include "SCUIIpc/message_manager.hpp"

namespace scui
{

bool IPCMessageManager::Initialize()
{
    // Initialization code here
    return true;
}

bool IPCMessageManager::Finalize()
{
    // Finalization code here
    return true;
}

IPCMessageManager* IPCMessageManager::Get()
{
    static IPCMessageManager instance;
    return &instance;
}

} // namespace scui