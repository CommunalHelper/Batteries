using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Batteries {

    [CustomEntity("batteries/battery_switch")]
    public class BatterySwitch : Solid {
        public static ParticleType P_Signal = new(DashSwitch.P_PressAMirror);

        private static bool particlesSetup = false;

        private readonly Sides side;
        private readonly bool persistent;
        private readonly bool allGates;
        private readonly Sprite sprite;
        private readonly bool alwaysFlag;
        private bool pressed;
        private Vector2 pressDirection;
        private EntityID id;

        public BatterySwitch(Vector2 position, Sides side, bool persistent, bool allGates, bool alwaysFlag, EntityID id)
            : base(position, 0f, 0f, safe: true) {
            if (!particlesSetup) {
                SetupParticles();
                particlesSetup = true;
            }

            this.side = side;
            this.persistent = persistent;
            this.allGates = allGates;
            this.id = id;
            this.alwaysFlag = alwaysFlag;
            Add(sprite = BatteriesModule.SpriteBank.Create("battery_switch"));
            if (side is Sides.Up or Sides.Down) {
                Collider.Width = 16f;
                Collider.Height = 8f;
            } else {
                Collider.Width = 8f;
                Collider.Height = 16f;
            }

            switch (side) {
                case Sides.Up:
                    sprite.Position = new Vector2(-2f, -2f);
                    sprite.Rotation = (float)Math.PI / 2f;
                    pressDirection = Vector2.UnitY;
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

        public BatterySwitch(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, direction(data), data.Bool("persistent"), data.Bool("allGates"), data.Bool("alwaysFlag"), id) { }

        public enum Sides {
            Up,
            Down,
            Left,
            Right
        }

        private string FlagName => GetFlagName(id);

        public static string GetFlagName(EntityID id) {
            return "batterySwitch_" + id.Key;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (persistent && SceneAs<Level>().Session.GetFlag(FlagName)) {
                sprite.Play("buzz");
                pressed = true;
                Collidable = true;
                LitState();
                foreach (BatteryGate entity in GetGate()) {
                    if (entity.closes) {
                        entity.StartClosed();
                    } else {
                        entity.StartOpen();
                    }
                }
            }
        }

        public override void Update() {
            base.Update();
            Level level = SceneAs<Level>();
            if (Scene.OnInterval(0.3f)) {
                if (pressed) {
                    level.ParticlesFG.Emit(DashSwitch.P_PressA, 1, Collider.AbsolutePosition + Collider.Center, pressDirection.Perpendicular() * 6f, (pressDirection * -1).Angle());
                } else {
                    Player player = level.Tracker.GetEntity<Player>();
                    if (player?.Holding?.Entity is Battery battery) {
                        if (battery.onlyFits == id.ID) {
                            level.ParticlesFG.Emit(P_Signal, 8, Collider.AbsolutePosition + Collider.Center + (pressDirection * -28), pressDirection.Perpendicular() * 6f, pressDirection.Angle());
                        }
                    }
                }
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            if (!persistent && alwaysFlag) {
                (scene as Level).Session.SetFlag(FlagName, false);
            }
        }

        public void Hit(Battery battery, Vector2 direction) {
            if (!pressed && direction == pressDirection && battery.Charge > 0 && (battery.onlyFits < 0 || battery.onlyFits == id.ID)) {
                battery.Use();
                battery.RemoveSelf();
                LitState();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("insert");
                pressed = true;
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collider.Collide(player)) {
                    player.Position += -5 * pressDirection;
                }

                SceneAs<Level>().ParticlesFG.Emit(DashSwitch.P_PressA, 10, Collider.AbsolutePosition + Collider.Center, direction.Perpendicular() * 6f, (pressDirection * -1).Angle());
                SceneAs<Level>().ParticlesFG.Emit(DashSwitch.P_PressB, 4, Collider.AbsolutePosition + Collider.Center, direction.Perpendicular() * 6f, (pressDirection * -1).Angle());
                if (allGates) {
                    foreach (BatteryGate entity in Scene.Tracker.GetEntities<BatteryGate>()) {
                        if (entity.entityID.Level == id.Level) {
                            entity.SwitchOpen();
                        }
                    }
                } else {
                    foreach (BatteryGate gate in GetGate()) {
                        gate.SwitchOpen();
                    }
                }

                Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(X - (Scene as Level).Bounds.Center.X));
                if (persistent || alwaysFlag) {
                    SceneAs<Level>().Session.SetFlag(FlagName);
                }
            }
        }

        private static void SetupParticles() {
            P_Signal.Color = Color.Aqua;
            P_Signal.ColorMode = ParticleType.ColorModes.Choose;
            P_Signal.LifeMax = 0.6f;
            P_Signal.LifeMin = 0.3f;
            P_Signal.SpeedMultiplier = 0.1f;
            P_Signal.DirectionRange = (float)(Math.PI / 5);
        }

        private static Sides direction(EntityData data) {
            return data.Bool("horizontal") ? data.Bool("rightSide") ? Sides.Right : Sides.Left : data.Bool("ceiling") ? Sides.Down : Sides.Up;
        }



        private void LitState() {
            switch (side) {
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

            Vector2 offset = side switch {
                Sides.Down => Collider.BottomCenter + new Vector2(0, 1),
                Sides.Up => Collider.TopCenter + new Vector2(0, -1),
                Sides.Right => Collider.CenterRight + new Vector2(1, 0),
                Sides.Left => Collider.CenterLeft + new Vector2(-1, 0),
                _ => Vector2.Zero
            };

            Add(new VertexLight(offset, Color.Lime, 1f, 12, 24));
        }

        private List<BatteryGate> GetGate() {
            List<Entity> entities = Scene.Tracker.GetEntities<BatteryGate>();
            List<BatteryGate> batteryGates = new();
            float num = 0f;

            foreach (BatteryGate item in entities) {
                if (!item.ClaimedByASwitch && item.bound && item.openWith == id.ID) {
                    batteryGates.Add(item);
                }
            }

            if (batteryGates.Count != 0) {
                return batteryGates;
            }

            BatteryGate batteryGate = null;
            foreach (BatteryGate item in entities) {
                if (!item.ClaimedByASwitch && item.entityID.Level == id.Level && !item.bound) {
                    float num2 = Vector2.DistanceSquared(Position, item.Position);
                    if (batteryGate == null || num2 < num) {
                        batteryGate = item;
                        num = num2;
                    }
                }
            }

            if (batteryGate != null) {
                batteryGate.ClaimedByASwitch = true;
                batteryGates.Add(batteryGate);
            }

            return batteryGates;
        }
    }
}
