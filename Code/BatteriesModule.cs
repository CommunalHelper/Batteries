using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.Batteries
{
    public class BatteriesModule : EverestModule
    {

        // Only one alive module instance can exist at any given time.
        public static BatteriesModule Instance;

        public static SpriteBank SpriteBank;

        public static Atlas Game;

        public BatteriesModule()
        {
            Instance = this;
        }

        // If you don't need to store any settings, => null
        public override Type SettingsType => null;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
        }


        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize()
        {
        }

        // Optional, do anything requiring either the Celeste or mod content here.
        public override void LoadContent(bool firstLoad)
        {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/BatteriesSprites.xml");
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload()
        {
        }

    }
}
