using System;

namespace RoguelikeMonoGame
{
    public sealed class WeaponData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "Weapon";
        public string Glyph { get; set; } = "w";
        public string Slot { get; set; } = "Weapon"; // keep your slot system
        public SpectrumVectorDto Emission { get; set; } = new();
        public WeaponUseData[] Uses { get; set; } = Array.Empty<WeaponUseData>();
    }

}
