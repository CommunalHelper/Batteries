local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local battery_switch = {}

battery_switch.name = "batteries/battery_switch"

local directions = {
    up = {false, "ceiling", false},
    down = {false, "ceiling", true},
    left = {true, "rightSide", false},
    right = {true, "rightSide", true},
}

battery_switch.placements = {}
for dir, data in pairs(directions) do
    local horiz, datakey, val = unpack(data)
    table.insert(battery_switch.placements, {
        name = dir,
        data = {
            persistent = false,
            alwaysFlag = false,
            ceiling = false,
            rightSide = false,
            horizontal = horiz,
            [datakey] = val --will override ceiling or rightSide
        }
    })
end

battery_switch.fieldOrder = {"x", "y", "horizontal", "rightSide", "ceiling", "persistent", "alwaysFlag"}

function battery_switch.sprite(room, entity)
    local texture = "batteries/battery_switch/insert8"
    local sprite = drawableSprite.fromTexture(texture, entity)

    if entity.horizontal then
        if entity.rightSide then
            sprite:addPosition(10, 8)
            sprite.rotation = math.pi
        else
            sprite:addPosition(-2, 8)
        end
    else
        if entity.ceiling then
            sprite:addPosition(8, 10)
            sprite.rotation = -math.pi / 2
        else
            sprite:addPosition(8, -2)
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
