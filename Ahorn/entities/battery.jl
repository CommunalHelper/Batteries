module BatteriesBattery

using ..Ahorn, Maple

@mapdef Entity "batteries/battery" Battery(x::Integer, y::Integer, maxCharge::Integer=500, initalCharge::Integer=500, dischargeRate::Integer=80, oneUse::Bool=false, onlyFits::Integer=-1, ignoreBarriers::Bool=false)

const placements = Ahorn.PlacementDict(
    "Battery (Batteries)" => Ahorn.EntityPlacement(
        Battery,
        "point"
    ),
    "Battery (Permanent) (Batteries)" => Ahorn.EntityPlacement(
        Battery,
        "point",
        Dict{String, Any}(
            "dischargeRate" => 0,
        )
    )
)

sprite = "batteries/battery/full0"

function Ahorn.selection(entity::Battery)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y - 8)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Battery, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -8)

end
