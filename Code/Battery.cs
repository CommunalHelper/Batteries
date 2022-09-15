using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using On.Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Batteries
{

    [Tracked]
    [CustomEntity("batteries/battery")]
    public class Battery : Actor
    {
        public static ParticleType P_Impact = TheoCrystal.P_Impact;

        public static ParticleType P_Infinite = new ParticleType(Refill.P_Glow);
        public static ParticleType P_Full = new ParticleType(Refill.P_Glow);
        public static ParticleType P_Half = new ParticleType(Refill.P_Glow);
        public static ParticleType P_Low = new ParticleType(Refill.P_Glow);

        public Vector2 Speed;

        public Holdable Hold;

        public static Battery LastHeld;

        private Sprite sprite;

        private bool dead;

        private Level Level;

        private Collision onCollideH;

        private Collision onCollideV;

        private float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private Vector2 previousPosition;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private float hardVerticalHitSoundCooldown = 0f;

        private static Vector2 particleOffset = new Vector2(0, -8);

        public bool Running = false;

        private ParticleType particle;

        public int MaxCharge;

        public int Charge;

        public int DischargeRate;

        private bool oneUse;

        private bool fresh;

        private bool ignoreBarriers;

        public int onlyFits;

        private EntityID id;

        private int spinnerHits = 0;

        private string FlagName => GetFlagName(id);

        public Battery(Vector2 position, int initalCharge, int maxCharge, int dischargeRate, bool oneUse, bool ignoreBarriers, int onlyFits, EntityID id)
            : base(position)
        {
            previousPosition = position;
            this.id = id;
            fresh = true;
            base.Depth = 100;
            base.Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = BatteriesModule.SpriteBank.Create("battery"));
            sprite.Scale.X = -1f;
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = HitSpinner;
            Hold.SpeedGetter = (() => Speed);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            Add(new MirrorReflection());
            particle = P_Full;
            this.MaxCharge = maxCharge;
            Charge = initalCharge;
            this.DischargeRate = dischargeRate;
            this.oneUse = oneUse;
            this.ignoreBarriers = ignoreBarriers;
            this.onlyFits = onlyFits;
            LoadParticles();
        }

        public Battery(EntityData e, Vector2 offset, EntityID id)
            : this(e.Position + offset, e.Int("initalCharge", 500), 
                   e.Int("maxCharge", 500), e.Int("dischargeRate", 80),
                   e.Bool("oneUse"), e.Bool("ignoreBarriers", false), 
                   e.Int("onlyFits", -1), id)
        {
        }

        public static void LoadParticles()
        {
            P_Infinite.Color = Color.Cyan;
            P_Infinite.ColorMode = ParticleType.ColorModes.Static;
            P_Full.Color = Color.Lime;
            P_Full.ColorMode = ParticleType.ColorModes.Static;
            P_Half.Color = Color.LightGoldenrodYellow;
            P_Half.ColorMode = ParticleType.ColorModes.Static;
            P_Low.Color = Color.OrangeRed;
            P_Low.ColorMode = ParticleType.ColorModes.Static;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            if (Level.Session.GetFlag(FlagName))
                RemoveSelf();
            string newSprite;
            ParticleType newParticle = P_Low;
            if (DischargeRate == 0)
            {
                newSprite = "infinite";
                newParticle = P_Infinite;
            }
            else if (fresh && Charge == MaxCharge)
            {
                newSprite = "fresh";
                newParticle = P_Full;
            }
            else if (Charge > MaxCharge / 2)
            {
                newSprite = "full";
                newParticle = P_Full;
            }
            else if (Charge > MaxCharge / 5)
            {
                newSprite = "half";
                newParticle = P_Half;
            }
            else if (Charge > 0)
            {
                newSprite = "low";
                newParticle = P_Low;
            }
            else
            {
                newSprite = "empty";
            }
            if (sprite.CurrentAnimationID != newSprite)
            {
                sprite.Play(newSprite);
                particle = newParticle;
            }
        }

        public void Reset()
        {
            if (Charge < MaxCharge * 0.80)
            {
                Audio.Play("event:/classic/sfx16", Position);
                Level.ParticlesFG.Emit(P_Full, 30, Position + particleOffset, Vector2.One * 10f);
            }
            Charge = MaxCharge;
        }

        public override void Update()
        {
            base.Update();
            if (Charge > 0 && base.Scene.OnInterval(fresh ? 0.5f : 0.1f))
            {
                bool collided = false;
                foreach (FakeWall wall in Scene.Tracker.GetEntities<FakeWall>())
                {
                    if (wall.Collider.Collide(this))
                    {
                        collided = true;
                    }
                }
                if (!collided)
                Level.ParticlesFG.Emit(particle, Hold.IsHeld ? 2 : 1, Position + particleOffset, Vector2.One * 5f);
            }
            if (dead)
            {
                return;
            }
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            base.Depth = 100;
            if (base.Scene.OnInterval(0.5f) && Running && Charge > 0)
            {
                Charge -= DischargeRate / 2;
            }
            string newSprite;
            ParticleType newParticle = P_Low;
            if (DischargeRate == 0)
            {
                newSprite = "infinite";
                newParticle = P_Infinite;
            }
            else if (fresh && Charge == MaxCharge)
            {
                newSprite = "fresh";
                newParticle = P_Full;
            }
            else if (Charge > MaxCharge / 2)
            {
                newSprite = "full";
                newParticle = P_Full;
            }
            else if (Charge > MaxCharge / 5)
            {
                newSprite = "half";
                newParticle = P_Half;
            }
            else if (Charge > 0)
            {
                newSprite = "low";
                newParticle = P_Low;
            }
            else
            {
                newSprite = "empty";
            }
            if (sprite.CurrentAnimationID != newSprite)
            {
                sprite.Play(newSprite);
                particle = newParticle;
                if (newSprite == "empty")
                {
                    Audio.Play("event:/classic/sfx0", Position);
                    Level.ParticlesFG.Emit(particle, 30, Position + particleOffset, Vector2.One * 10f);
                }
                else if (newSprite != "infinite" && newSprite != "full" && newSprite != "fresh")
                {
                    Audio.Play("event:/classic/sfx1", Position);
                }
            }
            if (Hold.IsHeld)
            {
                if (!Running)
                {
                    Running = true;
                    fresh = false;
                }
                prevLiftSpeed = Vector2.Zero;
                LastHeld = this;
            }
            else
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                previousPosition = base.ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (base.Right > (float)Level.Bounds.Right)
                {
                    base.Right = Level.Bounds.Right;
                    CollisionData data = new CollisionData
                    {
                        Direction = Vector2.UnitX
                    };
                    OnCollideH(data);
                }
                else if (base.Left < (float)Level.Bounds.Left)
                {
                    base.Left = Level.Bounds.Left;
                    CollisionData data = new CollisionData
                    {
                        Direction = -Vector2.UnitX
                    };
                    OnCollideH(data);
                }
                else if (base.Bottom > (float)Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    base.Bottom = Level.Bounds.Bottom;
                    Speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
                else if (base.Top > (float)Level.Bounds.Bottom)
                {
                    RemoveSelf();
                }
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                BatteryGate templeGate = CollideFirst<BatteryGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            }
            if (!dead)
            {
                foreach (BatteryCollider batteryCollider in base.Scene.Tracker.GetComponents<BatteryCollider>())
                {
                    batteryCollider.Check(this);
                }
                Hold.CheckAgainstColliders();
                if (!ignoreBarriers)
                {
                    foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
                    {
                        entity.Collidable = true;
                        bool flag = CollideCheck(entity);
                        entity.Collidable = false;
                        if (flag)
                        {
                            entity.OnReflectSeeker();
                            Die();
                        }
                    }
                }
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (!Hold.IsHeld)
            {
                Speed = (base.Center - from).SafeNormalize(120f);
                SlashFx.Burst(base.Center, Speed.Angle());
            }
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (base.Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        }

        public void HitSpinner(Entity spinner)
        {
            if (spinnerHits < 10 && !Hold.IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGround())
            {
                spinnerHits++;
                int num = Math.Sign(base.X - spinner.X);
                if (num == 0)
                {
                    num = 1;
                }
                Speed.X = (float)num * 120f;
                Speed.Y = -30f;
            }
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (data.Hit is BatterySwitch)
            {
                BatterySwitch batterySwitch = data.Hit as BatterySwitch;
                batterySwitch.Hit(this, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (data.Hit is BatterySwitch)
            {
                BatterySwitch batterySwitch = data.Hit as BatterySwitch;
                batterySwitch.Hit(this, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (data.Hit is RechargePlatform)
            {
                Reset();
                Running = false;
            }
            if (Speed.Y > 0f)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                }
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }
            Level.Particles.Emit(P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid)
        {
            return Speed.Y == 0f && base.IsRiding(solid);
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data) && !SaveData.Instance.Assists.Invincible)
            {
                Die();
            }
        }

        private void OnPickup()
        {
            spinnerHits = 0;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().Session.SetFlag(FlagName);
        }

        private void OnRelease(Vector2 force)
        {
            SceneAs<Level>().Session.SetFlag(FlagName, false);
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }

        public void Use()
        {
            if (oneUse)
            {
                SceneAs<Level>().Session.SetFlag(FlagName);
            }
        }

        public void Die()
        {
            if (!dead)
            {
                if (Hold.IsHeld)
                {
                    Vector2 speed2 = Hold.Holder.Speed;
                    Hold.Holder.Drop();
                    Speed = speed2 * 0.333f;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                dead = true;
                Audio.Play("event:/char/madeline/death", Position);
                Add(new DeathEffect(Color.ForestGreen, base.Center - Position));
                sprite.Visible = false;
                Charge = 0;
                base.Depth = -1000000;
                Collidable = false;
                AllowPushing = false;
            }
        }

        public static string GetFlagName(EntityID id)
        {
            return "battery_" + id.Key;
        }
    }


}
