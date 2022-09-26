local utils = require("utils")

local power_refill = {}

power_refill.name = "batteries/power_refill"
power_refill.depth = -100

power_refill.placements = {
    name = "default",
    data = {
        oneUse = false
    }
}

power_refill.texture = "batteries/power_refill/idle00"

function power_refill.selection(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 5, 11, 11)
end

return power_refill
