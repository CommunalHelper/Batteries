local battery = {}

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
            ignoreBarriers = false
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
            ignoreBarriers = false
        }
    }
}

battery.texture = "batteries/battery/full0"
battery.justification = {0.5, 1}

return battery
