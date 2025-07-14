#pragma once
/**
 * @file typed_message.hpp
 * @brief The Typed Message for IPC in SCUI
 * @author sailing-innocent
 * @date 2025-07-14
 */

#include "SkrContainersDef/optional.hpp"
#include "SkrContainers/vector.hpp"

#ifndef __meta__
    #include "SCUIIpc/typed_message.generated.h" // IWYU pragma: export
#endif

#define typed_message(uuid) sreflect_struct(guid = uuid serde = @json|@bin)
#define typed_message_enum(uuid) sreflect_enum_class(guid = uuid serde = @json|@bin)

namespace scui
{

typed_message("01980634-c8cf-703f-a33a-db4af6883a18")
    BinaryMessageData
{
    skr::Vector<uint8_t> data;
};

} // namespace scui