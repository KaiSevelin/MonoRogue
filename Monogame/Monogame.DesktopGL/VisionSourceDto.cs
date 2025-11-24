namespace RoguelikeMonoGame
{
    public sealed class VisionSourceDto
    {
        public string Mode { get; set; } = "Ambient"; // Ambient|Cone|Omni
        public int Radius { get; set; } = 10;
        public float ConeCenterDeg { get; set; } = 0;
        public float ConeHalfWidthDeg { get; set; } = 55;
        public SpectrumVectorDto Detector { get; set; } = new();
    }

}
