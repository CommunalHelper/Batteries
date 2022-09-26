local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local battery_switch = {}

battery_switch.name = "batteries/battery_switch"

battery_switch.placements = {
    {
        name = "up",
        data = {
            persistent = false,
            alwaysFlag = false,
            ceiling = false,
            rightSide = false,
            horizontal = false,
        }
    },
    {
        name = "down",
        data = {
            persistent = false,
            alwaysFlag = false,
            ceiling = true,
            rightSide = false,
            horizontal = false,
        }
    },
    {
        name = "left",
        data = {
            persistent = false,
            alwaysFlag = false,
            ceiling = false,
            rightSide = false,
            horizontal = true,
        }
    },
    {
        name = "right",
        data = {
            persistent = false,
            alwaysFlag = false,
            ceiling = false,
            rightSide = true,
            horizontal = true,
        }
    },
}

battery_switch.fieldOrder = {"x", "y", "horizontal", "rightSide", "ceiling", "persistent", "alwaysFlag"}

function battery_switch.sprite(room, entity)
    local texture = "batteries/battery_switch/insert8"
    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setJustification(0.5, 1)

    if entity.horizontal then
        if entity.rightSide then
            sprite:addPosition(10, 18)
            sprite:setScale(-1, 1)
        else
            sprite:addPosition(-2, 18)
        end
    else
        if entity.ceiling then
            sprite:addPosition(18, 10)
            sprite.rotation = -math.pi / 2
        else
            sprite:addPosition(-2, -2)
            sprite.rotation = math.pi / 2
        end
    end

    return sprite
end

function battery_switch.selection(room, entity)
    if entity.horizontal then
        if entity.rightSide then
            return utils.rectangle(entity.x, entity.y - 2, 13, 20)
        else
            return utils.rectangle(entity.x - 5, entity.y - 2, 13, 20)
        end
    else
        if entity.ceiling then
            return utils.rectangle(entity.x - 2, entity.y, 20, 13)
        else
            return utils.rectangle(entity.x - 2, entity.y - 5, 20, 13)
        end
    end
end

return battery_switch
