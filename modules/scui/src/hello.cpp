/**
 * @file hello.cpp
 * @brief The Sample App
 * @author sailing-innocent
 * @date 2025-07-11
 */

#include "SCUI/hello.h"
#include "SkrCore/log.h"

namespace scui
{

int hello()
{
    SKR_LOG_INFO(u8"Hello from SCUI!");
    return 2;
}

} // namespace scui