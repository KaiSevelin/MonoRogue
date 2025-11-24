using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class LightSourceItem : Item, IVision, IEmitsSpectra
    {
        public LightSourceData Data { get; }
        public SpectrumVector Emission { get; }

        public override string Glyph => Data.Glyph;
        public override string Slot => Data.Slot;
        public override bool CanEquip => true;

        // map
        public LightSourceItem(Point pos, LightSourceData data) : base(pos, data.Name)
        {
            Data = data;
            Emission = data.Emission.ToVector();
        }

        // inventory
        public LightSourceItem(LightSourceData data) : base(data.Name)
        {
            Data = data;
            Emission = data.Emission.ToVector();
        }

        public IEnumerable<VisionSource> GetVisionSources()
        {
            foreach (var s in Data.Sources)
                yield return s.ToVisionSource();
        }

        // default Item.Interact picks it up; OnEquip/OnUnequip optional
    }
}
