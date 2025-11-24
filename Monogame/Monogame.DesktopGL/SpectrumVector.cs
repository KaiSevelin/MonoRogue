using System;

namespace RoguelikeMonoGame
{
    public sealed class SpectrumVector
    {
        // Small, fast fixed vector (index by (int)SenseSpectrum)
        readonly int[] _v = new int[7];
        public int this[SenseSpectrum s] { get => _v[(int)s]; set => _v[(int)s] = Math.Max(0, value); }
        public void Clear() { Array.Clear(_v, 0, _v.Length); }
        public SpectrumVector Clone() { var n = new SpectrumVector(); Array.Copy(_v, n._v, _v.Length); return n; }
        public static SpectrumVector Zero => new SpectrumVector();
    }

}
