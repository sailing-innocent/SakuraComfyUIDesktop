#pragma once
/**
 * @file element.h
 * @brief The Shared Element between SCUI and python lib
 * @author sailing-innocent
 * @date 2025-07-13
 */

#include "SkrRT/resource/config_resource.h"

#ifndef __meta__
    #include "SCUI/element.generated.h" // IWYU pragma: export
#endif

namespace scui
{
sreflect_struct(
    guid = "01980356-b443-73c3-a7a1-002825d81bd8"
    serde = @bin|@json
) DummyElement {
    int id = 0;     // Unique identifier for the element
    float x = 0.0f; // X position of the element
};

} // namespace scui