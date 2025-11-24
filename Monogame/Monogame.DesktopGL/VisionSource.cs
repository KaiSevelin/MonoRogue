namespace RoguelikeMonoGame
{
    // A single vision / sensing source (from an item or a character)
    public sealed class VisionSource
    {
        public VisionMode Mode = VisionMode.Ambient;
        public int Radius = 10;                 // geometric radius cap (before spectrum math)
        public float ConeCenterDeg = 0f;        // used when Mode == Cone
        public float ConeHalfWidthDeg = 55f;    // used when Mode == Cone

        // For each spectrum, how good this source is at detecting it.
        public SpectrumVector Detector = new SpectrumVector();

        // For each spectrum, whether walls block/occlude rays.
        public bool BlocksByWalls(SenseSpectrum s) =>
            s switch
            {
                SenseSpectrum.Light => true,
                SenseSpectrum.Heat => true,     // simplified; you can tune
                SenseSpectrum.Scent => false,
                SenseSpectrum.Sound => false,
                SenseSpectrum.Vibration => false,
                SenseSpectrum.Evil => false,
                SenseSpectrum.Magic => false,
                _ => true
            };
    }

}
