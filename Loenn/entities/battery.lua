local battery = {}

local COLOR_CYAN = "00ffff"
local COLOR_FOREST_GREEN = "228b22"
local COLOR_LIGHT_GOLDENROD_YELLOW = "fafad2"
local COLOR_LIME = "00ff00"
local COLOR_ORANGE_RED = "ff4500"

battery.name = "batteries/battery"
battery.depth = 100

battery.placements = {
    {
        name = "default",
        data = {
            maxCharge = 500,
            initalCharge = 500, --not my typo
            dischargeRate = 80,
            oneUse = false,
            onlyFits = -1,
            ignoreBarriers = false,
            particleColorInfinite = COLOR_CYAN,
            particleColorFull = COLOR_LIME,
            particleColorHalf = COLOR_LIGHT_GOLDENROD_YELLOW,
            particleColorLow = COLOR_ORANGE_RED,
            deathEffectColor = COLOR_FOREST_GREEN
        }
    },
    {
        name = "permanent",
        data = {
            maxCharge = 500,
            initalCharge = 500,
            dischargeRate = 0,
            oneUse = false,
            onlyFits = -1,
            ignoreBarriers = false,
            particleColorInfinite = COLOR_CYAN,
            particleColorFull = COLOR_LIME,
            particleColorHalf = COLOR_LIGHT_GOLDENROD_YELLOW,
            particleColorLow = COLOR_ORANGE_RED,
            deathEffectColor = COLOR_FOREST_GREEN
        }
    }
}

battery.fieldInformation = {
    onlyFits = {
        fieldType = "integer"
    },
    particleColorInfinite = {
        fieldType = "color"
    },
    particleColorFull = {
        fieldType = "color"
    },
    particleColorHalf = {
        fieldType = "color"
    },
    particleColorLow = {
        fieldType = "color"
    },
    deathEffectColor = {
        fieldType = "color"
    }
}

battery.fieldOrder = {"x", "y", "maxCharge", "initalCharge", "dischargeRate", "onlyFits", "particleColorInfinite", "particleColorFull", "particleColorHalf", "particleColorLow", "deathEffectColor", "ignoreBarriers", "oneUse"}

battery.texture = "batteries/battery/full0"
battery.justification = {0.5, 1}

return battery
