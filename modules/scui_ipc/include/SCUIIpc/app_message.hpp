#pragma once
/**
 * @file app_message.hpp
 * @brief The Application Message for IPC in SCUI
 * @author sailing-innocent
 * @date 2025-07-14
 */

#include "SCUIIpc/typed_message.hpp"

#ifndef __meta__
    #include "SCUIIpc/app_message.generated.h" // IWYU pragma: export
#endif

namespace scui
{

sreflect_struct(guid="01980636-fba3-72ee-99a4-2cb9be4b8c77")
    ShutdownServerRequest {
    bool shutdown;
};

} // namespace scui