using System;

namespace RoguelikeMonoGame
{
    public sealed class LightSourceData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "Light";
        public string Glyph { get; set; } = "t";
        public string Slot { get; set; } = "Light";
        public VisionSourceDto[] Sources { get; set; } = Array.Empty<VisionSourceDto>();
        public SpectrumVectorDto Emission { get; set; } = new();
    }

}
