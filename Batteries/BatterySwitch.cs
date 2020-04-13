using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Batteries
{

    [CustomEntity("batteries/battery_switch")]
    public class BatterySwitch : Solid
    {
        public enum Sides
        {
            Up,
            Down,
            Left,
            Right
        }

        public static ParticleType P_PressA = DashSwitch.P_PressA;

        public static ParticleType P_PressB = DashSwitch.P_PressB;

        public static ParticleType P_Signal = DashSwitch.P_PressAMirror;

        private Sides side;

        private bool pressed;

        private Vector2 pressDirection;

        private float startY;

        private bool persistent;

        private EntityID id;

        private bool allGates;

        private Sprite sprite;

        private bool alwaysFlag;

        private static bool particlesSetup = false;

        private string FlagName => GetFlagName(id);

        public BatterySwitch(Vector2 position, Sides side, bool persistent, bool allGates, bool alwaysFlag, EntityID id)
            : base(position, 0f, 0f, safe: true)
        {
            if (!particlesSetup)
            {
                SetupParticles();
                particlesSetup = true;
            }
            this.side = side;
            this.persistent = persistent;
            this.allGates = allGates;
            this.id = id;
            this.alwaysFlag = alwaysFlag;
            Add(sprite = BatteriesModule.SpriteBank.Create("battery_switch"));
            if (side == Sides.Up || side == Sides.Down)
            {
                base.Collider.Width = 16f;
                base.Collider.Height = 8f;
            }
            else
            {
                base.Collider.Width = 8f;
                base.Collider.Height = 16f;
            }
            switch (side)
            {
                case Sides.Up:
                    sprite.Position = new Vector2(-2f, -2f);
                    sprite.Rotation = (float)Math.PI / 2f;
                    pressDirection = Vector2.UnitY;
                    startY = base.Y;
                    break;
                case Sides.Down:
                    sprite.Position = new Vector2(18f, 10f);
                    sprite.Rotation = -(float)Math.PI / 2f;
                    pressDirection = -Vector2.UnitY;
                    break;
                case Sides.Right:
                    sprite.Position = new Vector2(10f, 18f);
                    sprite.FlipX = true;
                    pressDirection = -Vector2.UnitX;
                    break;
                case Sides.Left:
                    sprite.Position = new Vector2(-2f, 18f);
                    sprite.FlipX = false;
                    pressDirection = Vector2.UnitX;
                    break;
            }
        }

        public BatterySwitch(EntityData data, Vector2 offset, EntityID id) : 
        this(data.Position + offset, direction(data), data.Bool("persistent"), data.Bool("allGates"), data.Bool("alwaysFlag"), id)
        { }

        private static void SetupParticles()
        {
            P_Signal.Color = Color.Aqua;
            P_Signal.ColorMode = ParticleType.ColorModes.Choose;
            P_Signal.LifeMax = 0.6f;
            P_Signal.LifeMin = 0.3f;
            P_Signal.SpeedMultiplier = 0.1f;
            P_Signal.DirectionRange = (float)(Math.PI / 5);
        }

        private static Sides direction(EntityData data)
        {
            if (data.Bool("horizontal"))
            {
                if (data.Bool("rightSide"))
                {
                    return Sides.Right;
                }
                return Sides.Left;
            }
            if (data.Bool("ceiling"))
            {
                return Sides.Down;
            }
            return Sides.Up;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (persistent && SceneAs<Level>().Session.GetFlag(FlagName))
            {
                sprite.Play("buzz");
                pressed = true;
                Collidable = true;
                LitState();
                foreach (BatteryGate entity in GetGate())
                {
                    if (entity.closes)
                    {
                        entity.StartClosed();
                    }
                    else
                    {
                        entity.StartOpen();
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            if (base.Scene.OnInterval(0.3f)) {
                if (pressed)
                {
                    level.ParticlesFG.Emit(P_PressA, 1, Collider.AbsolutePosition + Collider.Center, pressDirection.Perpendicular() * 6f, (pressDirection * -1).Angle());
                }
                else
                {
                    Holdable hold = level.Tracker.GetEntity<Player>().Holding;
                    if (hold != null && hold.Entity is Battery)
                    {
                        Battery battery = hold.Entity as Battery;
                        if (battery.onlyFits == id.ID)
                        {
                            level.ParticlesFG.Emit(P_Signal, 8, Collider.AbsolutePosition + Collider.Center + (pressDirection * -28), pressDirection.Perpendicular() * 6f, pressDirection.Angle());
                        }
                    }
                }
            }
        }

        private void LitState()
        {
            switch (side)
            {
                case Sides.Up:
                    Collider.Width = 16f;
                    Collider.Height = 12f;
                    Collider.Position.Y -= 4;
                    break;
                case Sides.Down:
                    Collider.Width = 16f;
                    Collider.Height = 12f;
                    break;
                case Sides.Left:
                    Collider.Width = 12f;
                    Collider.Height = 16f;
                    Collider.Position.X -= 4;
                    break;
                case Sides.Right:
                    Collider.Width = 12f;
                    Collider.Height = 16f;
                    break;
            }
            Vector2 offset = Vector2.Zero;
            switch (side) {
                case Sides.Down:
                    offset = base.Collider.BottomCenter + new Vector2(0, 1);
                    break;
                case Sides.Up:
                    offset = base.Collider.TopCenter + new Vector2(0, -1);
                    break;
                case Sides.Right:
                    offset = base.Collider.CenterRight + new Vector2(1, 0);
                    break;
                case Sides.Left:
                    offset = base.Collider.CenterLeft + new Vector2(-1, 0);
                    break;
            }
            Add(new VertexLight(offset, Color.Lime, 1f, 12, 24));
        }

        public void Hit(Battery battery, Vector2 direction)
        {
            if (!pressed && direction == pressDirection && battery.Charge > 0 && (battery.onlyFits < 0 || battery.onlyFits == id.ID))
            {
                battery.Use();
                battery.RemoveSelf();
                LitState();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("insert");
                pressed = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collider.Collide(player))
                {
                    player.Position += -5 * pressDirection;
                }
                SceneAs<Level>().ParticlesFG.Emit(P_PressA, 10, Collider.AbsolutePosition + Collider.Center, direction.Perpendicular() * 6f, (pressDirection * -1).Angle());
                SceneAs<Level>().ParticlesFG.Emit(P_PressB, 4, Collider.AbsolutePosition + Collider.Center, direction.Perpendicular() * 6f, (pressDirection * -1).Angle());
                if (allGates)
                {
                    foreach (BatteryGate entity in base.Scene.Tracker.GetEntities<BatteryGate>())
                    {
                        if (entity.entityID.Level == id.Level)
                        {
                            entity.SwitchOpen();
                        }
                    }
                }
                else
                {
                    foreach (BatteryGate gate in GetGate()) {
                        gate.SwitchOpen();
                    }
                }
                base.Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(base.X - (float)(base.Scene as Level).Bounds.Center.X));
                if (persistent || alwaysFlag)
                {
                    SceneAs<Level>().Session.SetFlag(FlagName);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (!persistent && alwaysFlag)
            {
                SceneAs<Level>().Session.SetFlag(FlagName, false);
            }
        }

        private List<BatteryGate> GetGate()
        {
            List<Entity> entities = base.Scene.Tracker.GetEntities<BatteryGate>();
            List<BatteryGate> batteryGates = new List<BatteryGate>();
            float num = 0f;

            foreach (BatteryGate item in entities)
            {
                if (!item.ClaimedByASwitch && item.bound && item.openWith == id.ID)
                {
                    batteryGates.Add(item);
                }
            }
            if (batteryGates.Count != 0)
            {
                return batteryGates;
            }
            BatteryGate batteryGate = null;
            foreach (BatteryGate item in entities)
            {
                if (!item.ClaimedByASwitch && item.entityID.Level == id.Level && !item.bound)
                {
                    float num2 = Vector2.DistanceSquared(Position, item.Position);
                    if (batteryGate == null || num2 < num)
                    {
                        batteryGate = item;
                        num = num2;
                    }
                }
            }
            if (batteryGate != null)
            {
                batteryGate.ClaimedByASwitch = true;
                batteryGates.Add(batteryGate);
            }
            return batteryGates;
        }

        public static string GetFlagName(EntityID id)
        {
            return "batterySwitch_" + id.Key;
        }
    }

}
