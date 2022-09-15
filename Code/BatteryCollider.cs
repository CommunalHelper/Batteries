using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Batteries
{
    [Tracked(false)]
    public class BatteryCollider : Component
    {
        public Action<Battery> OnCollide;

        public Collider Collider;

        public BatteryCollider(Action<Battery> onCollide, Collider collider = null)
            : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = collider;
        }

        public bool Check(Battery battery)
        {
            Collider collider = Collider;
            if (collider == null)
            {
                if (battery.CollideCheck(base.Entity))
                {
                    OnCollide(battery);
                    return true;
                }
                return false;
            }
            Collider collider2 = base.Entity.Collider;
            base.Entity.Collider = collider;
            bool flag = battery.CollideCheck(base.Entity);
            base.Entity.Collider = collider2;
            if (flag)
            {
                OnCollide(battery);
                return true;
            }
            return false;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Collider collider = base.Entity.Collider;
                base.Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                base.Entity.Collider = collider;
            }
        }
    }

}
