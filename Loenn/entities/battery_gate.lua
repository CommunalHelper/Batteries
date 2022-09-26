local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local battery_gate = {}

battery_gate.name = "batteries/battery_gate"
battery_gate.depth = -9000

battery_gate.placements = {
    {
        name = "vertical",
        data = {
            switchId = -1,
            vertical = true,
            closes = false
        }
    },
    {
        name = "vertical_closing",
        data = {
            switchId = -1,
            vertical = true,
            closes = true
        }
    },
    {
        name = "horizontal",
        data = {
            switchId = -1,
            vertical = false,
            closes = false
        }
    },
    {
        name = "horizontal_closing",
        data = {
            switchId = -1,
            vertical = false,
            closes = true
        }
    },
}

battery_gate.fieldInformation = {
    switchId = {
        fieldType = "integer"
    }
}

battery_gate.canResize = {false, false} -- prevent resizing gates placed with old Ahorn plugin

function battery_gate.sprite(room, entity)
    local texture = (entity.closes ? "batteries/battery_gate/door15" : "batteries/battery_gate/door1")
    local sprite = drawableSprite.fromTexture(texture, entity)

    if entity.vertical then
        sprite:addPosition(7, 24)
    else
        sprite:addPosition(24, 8)
        sprite.rotation = -math.pi / 2
    end

    return sprite
end

function battery_gate.selection(room, entity)
    if entity.vertical then
        return utils.rectangle(entity.x, entity.y, 15, 48)
    else
        return utils.rectangle(entity.x, entity.y, 48, 15)
    end
end

return battery_gate
