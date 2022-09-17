using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Batteries {
    [CustomEntity("batteries/power_refill")]
    public class PowerRefill : Entity {
        private readonly Sprite sprite;
        private readonly Sprite flash;
        private readonly Image outline;
        private readonly Wiggler wiggler;
        private readonly BloomPoint bloom;
        private readonly VertexLight light;
        private readonly SineWave sine;
        private readonly bool oneUse;
        private Level level;
        private float respawnTimer;

        public PowerRefill(Vector2 position, bool oneUse)
            : base(position) {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new BatteryCollider(OnBattery));
            this.oneUse = oneUse;
            string str;
            str = "batteries/power_refill/";

            Add(outline = new Image(GFX.Game[str + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, str + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v) {
                sprite.Scale = flash.Scale = Vector2.One * (1f + (v * 0.2f));
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f, 0f));
            sine.Randomize();
            UpdateY();
            Depth = -100;
        }

        public PowerRefill(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("oneUse")) {
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update() {
            base.Update();
            if (respawnTimer > 0f) {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f) {
                    Respawn();
                }
            } else if (Scene.OnInterval(0.1f)) {
                level.ParticlesFG.Emit(Refill.P_Glow, 1, Position, Vector2.One * 5f);
            }

            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (Scene.OnInterval(2f) && sprite.Visible) {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn() {
            if (!Collidable) {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(Refill.P_Regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY() {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = bloom.Y = sine.Value * 2f;
            obj.Y = obj2.Y = num2;
        }

        private void OnBattery(Battery battery) {
            if (battery.DischargeRate > 0 && battery.Charge < battery.MaxCharge * 0.80 && battery.Charge > 0) {
                battery.Reset();
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(battery)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Battery battery) {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = flash.Visible = false;
            outline.Visible = !oneUse;

            Depth = 8999;
            yield return 0.05f;
            float angle = battery.Speed.Angle();
            level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle - ((float)Math.PI / 2f));
            level.ParticlesFG.Emit(Refill.P_Shatter, 5, Position, Vector2.One * 4f, angle + ((float)Math.PI / 2f));
            SlashFx.Burst(Position, angle);
            if (oneUse) {
                RemoveSelf();
            }
        }
    }
}
