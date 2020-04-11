using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.Entities;

namespace Celeste.Mod.Batteries
{

    [Tracked(false)]
    [CustomEntity("batteries/battery_gate")]
    public class BatteryGate : Solid
    {

        private bool vertical;

        private const int OpenHeight = 4;

        private const float HoldingWaitTime = 0.2f;

        private const float HoldingOpenDistSq = 4096f;

        private const float HoldingCloseDistSq = 6400f;

        private const int MinDrawHeight = 4;

        public EntityID entityID;

        public bool ClaimedByASwitch;

        private int closedSize;

        private Sprite sprite;

        private Shaker shaker;

        private float drawHeight;

        private float drawHeightMoveSpeed;

        private Vector2 holdingCheckFrom;

        public bool bound;

        public bool closes;

        public int openWith;
        
        public BatteryGate(Vector2 position, int size, bool vertical, int? openWith, bool closes, EntityID id)
            : base(position, vertical ? 15f : size, vertical ? size : 15f, safe: true)
        {
            this.vertical = vertical;
            this.closes = closes;
            bound = !(openWith == null || openWith < 0);
            if (bound)
                this.openWith = (int)openWith;
            closedSize = size;
            this.entityID = id;
            Add(sprite = BatteriesModule.SpriteBank.Create("battery_gate"));
            if (!vertical)
            {
                sprite.Rotation = 3*(float)Math.PI / 2;
            }
            if (vertical)
            {
                sprite.X = (base.Collider.Width - 1) / 2;
            }
            else
            {
                sprite.Y = (base.Collider.Height + 1) / 2;
            }
            sprite.Play("idle");
            Add(shaker = new Shaker(on: false));
            if (closes)
                StartOpen();
            base.Depth = -9000;
            holdingCheckFrom = Position + (vertical ? new Vector2(base.Width / 2f, size / 2) : new Vector2(size / 2, base.Width / 2f));
        }

        public BatteryGate(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.Height, data.Bool("vertical"), data.Int("switchId", -1), data.Bool("closes", false), id)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            drawHeight = Math.Max(4f, vertical ? base.Height : base.Width);
        }

        public void SwitchOpen()
        {
            sprite.Play("open");
            Alarm.Set(this, 0.2f, delegate
            {
                shaker.ShakeFor(0.2f, removeOnFinish: false);
                if (closes)
                {
                    Alarm.Set(this, 0.2f, Close);
                } else
                {
                    Alarm.Set(this, 0.2f, Open);
                }
            });
        }

        public void Open()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_main_open", Position);
            drawHeightMoveSpeed = 200f;
            drawHeight = vertical ? base.Height : base.Width;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(OpenHeight);
            sprite.Play("open");
        }

        public void StartOpen()
        {
            SetHeight(OpenHeight);
            drawHeight = 4f;
        }

        public void StartClosed()
        {
            SetHeight(closedSize);
            drawHeight = Math.Max(4f, vertical ? base.Height : base.Width);
        }

        public void Close()
        {
            Audio.Play("event:/game/05_mirror_temple/gate_main_close", Position);
            drawHeightMoveSpeed = 400f;
            drawHeight = Math.Max(4f, vertical ? base.Height : base.Width);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedSize);
            sprite.Play("hit");
        }

        private void SetHeight(int height)
        {
            if (vertical)
            {
                if ((float)height < base.Collider.Height)
                {
                    base.Collider.Height = height;
                    return;
                }
                float y = base.Y;
                int num = (int)base.Collider.Height;
                if (base.Collider.Height < 64f)
                {
                    base.Y -= 64f - base.Collider.Height;
                    base.Collider.Height = 64f;
                }
                MoveVExact(height - num);
                base.Y = y;
                base.Collider.Height = height;
            }
            else
            {
                if ((float)height < base.Collider.Width)
                {
                    base.Collider.Width = height;
                    return;
                }
                float x = base.X;
                int num = (int)base.Collider.Width;
                if (base.Collider.Width < 64f)
                {
                    base.X -= 64f - base.Collider.Width;
                    base.Collider.Width = 64f;
                }
                MoveHExact(height - num);
                base.X = x;
                base.Collider.Width = height;
            }
        }

        public override void Update()
        {
            base.Update();
            float num = Math.Max(4f, vertical ? base.Height : base.Width);
            if (drawHeight != num)
            {
                drawHeight = Calc.Approach(drawHeight, num, drawHeightMoveSpeed * Engine.DeltaTime);
            }
        }

        public override void Render()
        {
            if (vertical)
            {
                Vector2 value = new Vector2(Math.Sign(shaker.Value.X), 0f);
                Draw.Rect(base.X+2, base.Y-1, 11f, 2f, Color.Black);
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            }
            else
            {
                Vector2 value = new Vector2(0f, Math.Sign(shaker.Value.Y));
                Draw.Rect(base.X-1, base.Y+2, 2f, 11f, Color.Black);
                sprite.DrawSubrect(Vector2.Zero + value, new Rectangle(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight));
            }
        }
    }

}
