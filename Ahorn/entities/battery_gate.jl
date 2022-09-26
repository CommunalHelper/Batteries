module BatteriesBatteryGate

using ..Ahorn, Maple

@mapdef Entity "batteries/battery_gate" BatteryGate(x::Integer, y::Integer, vertical::Bool=true, closes::Bool=false, switchId::Integer=-1)

const placements = Ahorn.PlacementDict(
    "Battery Gate (Vertical) (Batteries)" => Ahorn.EntityPlacement(
        BatteryGate,
        "point",
        Dict{String, Any}(
            "vertical" => true,
        )
    ),
    "Battery Gate (Horizontal) (Batteries)" => Ahorn.EntityPlacement(
        BatteryGate,
        "point",
        Dict{String, Any}(
            "vertical" => false,
        )
    ),
    "Closing Battery Gate (Vertical) (Batteries)" => Ahorn.EntityPlacement(
        BatteryGate,
        "point",
        Dict{String, Any}(
            "vertical" => true,
            "closes" => true,
        )
    ),
    "Closing Battery Gate (Horizontal) (Batteries)" => Ahorn.EntityPlacement(
        BatteryGate,
        "point",
        Dict{String, Any}(
            "vertical" => false,
            "closes" => true,
        )
    ),
)

function Ahorn.selection(entity::BatteryGate)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 48))

    if get(entity.data, "vertical", true)
        return Ahorn.Rectangle(x, y, 15, height)
    else
        return Ahorn.Rectangle(x, y, height, 15)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BatteryGate, room::Maple.Room)
    if get(entity.data, "closes", false)
        sprite = "batteries/battery_gate/door15"
    else
        sprite = "batteries/battery_gate/door1"
    end

    if get(entity.data, "vertical", true)
        Ahorn.drawSprite(ctx, sprite, 7, 24)
    else
        Ahorn.drawSprite(ctx, sprite, 12, 44, rot=-pi / 2)
    end
end

end
