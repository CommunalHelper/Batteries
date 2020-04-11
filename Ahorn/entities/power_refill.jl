module BatteriesPowerRefill

using ..Ahorn, Maple

@mapdef Entity "batteries/power_refill" PowerRefill(x::Integer, y::Integer, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "Battery Power Refill" => Ahorn.EntityPlacement(
        PowerRefill
    ),
)

spriteOneDash = "batteries/power_refill/idle00"

function getSprite(entity::PowerRefill)
    return spriteOneDash
end

function Ahorn.selection(entity::PowerRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PowerRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
