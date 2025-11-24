using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public static class ItemRegistry
    {
        // In a real build, load these from JSON files at startup.
        public static readonly Dictionary<string, LightSourceData> Lights = new();
        public static readonly Dictionary<string, WeaponData> Weapons = new();
        public static void BootstrapDefaults()
        {
            // Torch (Light)
            ItemRegistry.Lights["torch"] = new LightSourceData
            {
                Id = "torch",
                Name = "Torch",
                Glyph = "t",
                Slot = "Light",
                Emission = new SpectrumVectorDto { Light = 6, Heat = 3, Scent = 1 },
                Sources = new[]
                {
                    new VisionSourceDto { Mode = "Ambient", Radius = 9,  Detector = new SpectrumVectorDto { Light = 10 } },
                    new VisionSourceDto { Mode = "Cone",    Radius = 10, ConeCenterDeg = 0, ConeHalfWidthDeg = 55, Detector = new SpectrumVectorDto { Light = 8 } }
                }
            };

            // Sword (melee)
            ItemRegistry.Weapons["sword"] = new WeaponData
            {
                Id = "sword",
                Name = "Sword",
                Glyph = "s",
                Slot = "Weapon",
                Uses = new[]
                {
                    new WeaponUseData { Type = "Slash", Damage = 6 },
                    new WeaponUseData { Type = "Stab",  Damage = 5 }
                }
            };

            // Bow (ranged)
            ItemRegistry.Weapons["bow"] = new WeaponData
            {
                Id = "bow",
                Name = "Bow",
                Glyph = "b",
                Slot = "Weapon",
                Uses = new[]
                {
                    new WeaponUseData { Type = "Shoot", Damage = 4, Range = 10, ProjectileGlyph = "→", AmmoItemId = "" }
                }
            };
        }
    }
}