using System;

namespace RoguelikeMonoGame
{
    public sealed class RangeInt
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public RangeInt() { }
        public RangeInt(int min, int max) { Min = min; Max = max; }
        public int Next(Random rng) => (Min == Max) ? Min : rng.Next(Min, Max + 1);
    }

}
