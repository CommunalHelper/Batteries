using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Batteries {
    [CustomEntity("batteries/recharge_platform")]
    public class RechargePlatform : Solid {
        private readonly Collider nc;

        public RechargePlatform(Vector2 Position) 
            : base(Position, 24, 4, safe: true) {
            Add(BatteriesModule.SpriteBank.Create("recharge_platform"));
            Collider.BottomCenter = Vector2.Zero;
            Add(new PlayerCollider(OnPlayerCollide, nc = Collider.Clone()));
            nc.Width += 2;
            nc.Height += 3;
            nc.CenterX -= 1;
            nc.CenterY -= 3;
        }

        public RechargePlatform(EntityData data, Vector2 offset) 
            : this(data.Position + offset) { 
        }

        public void OnPlayerCollide(Player player) {
            if ((Math.Abs(player.Speed.X) > 0f || HasPlayerClimbing()) && Math.Abs(player.Position.Y - Position.Y) < 0.5) {
                player.Position.Y = Position.Y - 4;
                player.Position.X += Math.Sign(Position.X - player.Position.X);
            }

            if (Battery.LastHeld != null && Battery.LastHeld.Hold.IsHeld) {
                Battery.LastHeld.Reset();
            }
        }
    }
}
