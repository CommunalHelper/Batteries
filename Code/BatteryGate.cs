using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Batteries {

    [Tracked(false)]
    [CustomEntity("batteries/battery_gate")]
    public class BatteryGate : Solid {
        public EntityID entityID;
        public bool ClaimedByASwitch;
        public bool bound;
        public bool closes;
        public int openWith;

        private const int OpenHeight = 4;
        private readonly bool vertical;
        private readonly int closedSize;
        private readonly Sprite sprite;
        private readonly Shaker shaker;
        private float drawHeight;
        private float drawHeightMoveSpeed;

        public BatteryGate(Vector2 position, int size, bool vertical, int? openWith, bool closes, EntityID id)
            : base(position, vertical ? 15f : size, vertical ? size : 15f, safe: true) {
            this.vertical = vertical;
            this.closes = closes;
            bound = openWith is not (null or < 0);
            if (bound) {
                this.openWith = (int)openWith;
            }

            closedSize = size;
            entityID = id;
            Add(sprite = BatteriesModule.SpriteBank.Create("battery_gate"));

            if (vertical) {
                sprite.X = (Collider.Width - 1) / 2;
            } else {
                sprite.Y = (Collider.Height + 1) / 2;
                sprite.Rotation = 3 * (float)Math.PI / 2;
            }

            sprite.Play("idle");
            Add(shaker = new Shaker(on: false));
            if (closes) {
                StartOpen();
            }

            Depth = -9000;
        }

        public BatteryGate(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Height, data.Bool("vertical"), data.Int("switchId", -1), data.Bool("closes", false), id) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            drawHeight = Math.Max(4f, vertical ? Height : Width);
        }

        public override void Update() {
            base.Update();
            float num = Math.Max(4f, vertical ? Height : Width);
            if (drawHeight != num) {
                drawHeight = Calc.Approach(drawHeight, num, drawHeightMoveSpeed * Engine.DeltaTime);
            }
        }

        public override void Render() {
            if (vertical) {
                Vector2 value = new(Math.Sign(shaker.Value.X), 0f);
                Draw.Rect(X + 2, Y - 1, 11f, 2f, Color.Black);
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            } else {
                Vector2 value = new(0f, Math.Sign(shaker.Value.Y));
                Draw.Rect(X - 1, Y + 2, 2f, 11f, Color.Black);
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            }
        }

        public void SwitchOpen() {
            sprite.Play("open");
            Alarm.Set(this, 0.2f, delegate {
                shaker.ShakeFor(0.2f, removeOnFinish: false);
                Alarm.Set(this, 0.2f, closes ? Close : Open);
            });
        }

        public void Open() {
            Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
            drawHeightMoveSpeed = 200f;
            drawHeight = vertical ? Height : Width;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(OpenHeight);
            sprite.Play("open");
        }

        public void StartOpen() {
            SetHeight(OpenHeight);
            drawHeight = 4f;
        }

        public void StartClosed() {
            SetHeight(closedSize);
            drawHeight = Math.Max(4f, vertical ? Height : Width);
        }

        public void Close() {
            Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
            drawHeightMoveSpeed = 400f;
            drawHeight = Math.Max(4f, vertical ? Height : Width);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedSize);
            sprite.Play("hit");
        }

        private void SetHeight(int height) {
            if (vertical) {
                if (height < Collider.Height) {
                    Collider.Height = height;
                    return;
                }

                float y = Y;
                int num = (int)Collider.Height;
                if (Collider.Height < 64f) {
                    Y -= 64f - Collider.Height;
                    Collider.Height = 64f;
                }

                MoveVExact(height - num);
                Y = y;
                Collider.Height = height;
            } else {
                if (height < Collider.Width) {
                    Collider.Width = height;
                    return;
                }

                float x = X;
                int num = (int)Collider.Width;
                if (Collider.Width < 64f) {
                    X -= 64f - Collider.Width;
                    Collider.Width = 64f;
                }

                MoveHExact(height - num);
                X = x;
                Collider.Width = height;
            }
        }
    }
}
