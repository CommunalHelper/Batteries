module BatteriesRechargePlatform

using ..Ahorn, Maple

@mapdef Entity "batteries/recharge_platform" BatteryRechargePlatform(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Battery Recharge Platform (Batteries)" => Ahorn.EntityPlacement(
        BatteryRechargePlatform
    )
)

sprite = "batteries/recharge_platform/base0"

function Ahorn.selection(entity::BatteryRechargePlatform)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y - 4)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BatteryRechargePlatform, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -4)

end
