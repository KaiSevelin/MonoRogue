using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeMonoGame
{
    public sealed class WeaponItem : DataBackedItem
    {
        public WeaponData Data { get; }
        public SpectrumVector Emission { get; } = new SpectrumVector();

        public override string Glyph => Data.Glyph;
        public override string Slot => Data.Slot;
        public override bool CanEquip => true;

        private readonly List<IUse> _uses = new();

        public WeaponItem(Point pos, WeaponData data)
            : base(pos, data.Name, data.Id)
        {
            Data = data;
            Emission = data.Emission.ToVector();
            BuildUses();
        }

        public WeaponItem(WeaponData data)
            : base(data.Name, data.Id)
        {
            Data = data;
            Emission = data.Emission.ToVector();
            BuildUses();
        }

        void BuildUses()
        {
            foreach (var u in Data.Uses)
            {
                switch (u.Type)
                {
                    case "Stab": _uses.Add(new StabUse(u.Damage)); break;
                    case "Slash": _uses.Add(new SlashUse(u.Damage)); break;
                    case "Throw": _uses.Add(new ThrowUse(u.Damage, u.Range, u.ProjectileGlyph, u.ConsumesItem)); break;
                    case "Shoot": _uses.Add(new ShootUse(u.Damage, u.Range, u.ProjectileGlyph, u.AmmoItemId)); break;
                    default: break; // unknown use type => ignore
                }
            }
        }

        public IEnumerable<IUse> Uses => _uses;

        public int Damage { get; internal set; }
        public List<string> Tags { get; internal set; } = new();

        // Convenience: perform the first use, or by id
        public bool PerformDefaultUse(Character user, DungeonMap map, List<NonPlayerCharacter> npcs,
                                      Point dir, Random rng)
            => _uses.Count > 0 && _uses[0].Perform(user, map, npcs, dir, rng);

        public bool PerformUse(string useId, Character user, DungeonMap map, List<NonPlayerCharacter> npcs,
                                Point dir, Random rng)
            => _uses.FirstOrDefault(u => string.Equals(u.Id, useId, StringComparison.OrdinalIgnoreCase))
               ?.Perform(user, map, npcs,  dir, rng) == true;
    }

}
