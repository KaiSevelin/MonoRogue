using System;

namespace RoguelikeMonoGame
{
    static class DtoMap
    {
        public static SpectrumVector ToVector(this SpectrumVectorDto dto)
        {
            var v = new SpectrumVector();
            v[SenseSpectrum.Light] = dto.Light;
            v[SenseSpectrum.Heat] = dto.Heat;
            v[SenseSpectrum.Scent] = dto.Scent;
            v[SenseSpectrum.Sound] = dto.Sound;
            v[SenseSpectrum.Vibration] = dto.Vibration;
            v[SenseSpectrum.Evil] = dto.Evil;
            v[SenseSpectrum.Magic] = dto.Magic;
            return v;
        }

        public static VisionSource ToVisionSource(this VisionSourceDto dto)
        {
            var mode = dto.Mode.Equals("Cone", StringComparison.OrdinalIgnoreCase) ? VisionMode.Cone
                      : dto.Mode.Equals("Omni", StringComparison.OrdinalIgnoreCase) ? VisionMode.Omni
                      : VisionMode.Ambient;

            return new VisionSource
            {
                Mode = mode,
                Radius = dto.Radius,
                ConeCenterDeg = dto.ConeCenterDeg,
                ConeHalfWidthDeg = dto.ConeHalfWidthDeg,
                Detector = dto.Detector.ToVector()
            };
        }
    }
}
