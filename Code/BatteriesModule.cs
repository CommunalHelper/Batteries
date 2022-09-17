using Monocle;

namespace Celeste.Mod.Batteries {
    public class BatteriesModule : EverestModule {
        public static BatteriesModule Instance;
        public static SpriteBank SpriteBank;

        public BatteriesModule() {
            Instance = this;
        }

        public override void Load() { }

        public override void Unload() { }

        public override void LoadContent(bool firstLoad) {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/BatteriesSprites.xml");
        }
    }
}
