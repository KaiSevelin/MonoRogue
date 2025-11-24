using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    // Equipment and characters with sensing must implement this.
    public interface IVision
    {
        // Return one or more vision sources this object provides.
        // (e.g., a Torch returns a Light source; “Elven Eyes” might return Heat+Light)
        IEnumerable<VisionSource> GetVisionSources();
    }

}
