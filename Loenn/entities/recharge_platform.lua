local utils = require("utils")

local recharge_platform = {}

recharge_platform.name = "batteries/recharge_platform"

recharge_platform.placements = {name = "default"}

recharge_platform.texture = "batteries/recharge_platform/base0"
recharge_platform.justification = {0.5, 1}

function recharge_platform.selection(room, entity)
    return utils.rectangle(entity.x - 11, entity.y - 4, 22, 4)
end

return recharge_platform
