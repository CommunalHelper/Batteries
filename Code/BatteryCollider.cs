using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Batteries {
    [Tracked(false)]
    public class BatteryCollider : Component {
        public Action<Battery> OnCollide;
        public Collider Collider;

        public BatteryCollider(Action<Battery> onCollide, Collider collider = null)
            : base(active: false, visible: false) {
            OnCollide = onCollide;
            Collider = collider;
        }

        public override void DebugRender(Camera camera) {
            if (Collider != null) {
                Collider collider = Entity.Collider;
                Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                Entity.Collider = collider;
            }
        }

        public bool Check(Battery battery) {
            Collider collider = Collider;
            if (Collider == null) {
                if (battery.CollideCheck(Entity)) {
                    OnCollide(battery);
                    return true;
                }

                return false;
            }

            Collider collider2 = Entity.Collider;
            Entity.Collider = collider;
            bool flag = battery.CollideCheck(Entity);
            Entity.Collider = collider2;
            if (flag) {
                OnCollide(battery);
                return true;
            }

            return false;
        }
    }
}
