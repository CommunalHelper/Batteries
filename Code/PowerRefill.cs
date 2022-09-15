using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Batteries
{
    [CustomEntity("batteries/power_refill")]
    public class PowerRefill : Entity
    {
        public static ParticleType P_Shatter = Refill.P_Shatter;

        public static ParticleType P_Regen = Refill.P_Regen;

        public static ParticleType P_Glow = Refill.P_Glow;

        public static ParticleType P_ShatterTwo = Refill.P_ShatterTwo;

        public static ParticleType P_RegenTwo = Refill.P_RegenTwo;

        public static ParticleType P_GlowTwo = Refill.P_GlowTwo;

        private Sprite sprite;

        private Sprite flash;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        public PowerRefill(Vector2 position, bool oneUse)
            : base(position)
        {
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new BatteryCollider(OnBattery));
            this.oneUse = oneUse;
            string str;
            str = "batteries/power_refill/";
            p_shatter = P_Shatter;
            p_regen = P_Regen;
            p_glow = P_Glow;
            Add(outline = new Image(GFX.Game[str + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            base.Depth = -100;
        }

        public PowerRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (base.Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (base.Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                base.Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = bloom.Y = sine.Value * 2f;
            float num5 = obj.Y = (obj2.Y = num2);
        }

        public override void Render()
        {
            base.Render();
        }

        private void OnBattery(Battery battery)
        {
            if (battery.DischargeRate > 0 && battery.Charge < battery.MaxCharge * 0.80 && battery.Charge > 0)
            {
                battery.Reset();
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(battery)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Battery battery)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = (flash.Visible = false);
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float angle = battery.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, angle - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, angle + (float)Math.PI / 2f);
            SlashFx.Burst(Position, angle);
            if (oneUse)
            {
                RemoveSelf();
            }
        }
    }

}
