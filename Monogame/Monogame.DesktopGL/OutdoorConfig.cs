using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Configuration for an outdoor region: can control houses, vegetation,
    /// sea coverage and number of dungeon portals.
    /// </summary>
    public sealed class OutdoorConfig
    {
        public int Width { get; }
        public int Height { get; }

        /// <summary>Whether this outdoor region has a settlement / houses.</summary>
        public bool HasHouses { get; set; } = true;

        /// <summary>0..1, how dense vegetation (trees, bushes) should be.</summary>
        public float VegetationDensity { get; set; } = 0.5f;

        /// <summary>0..1, fraction of the area that should become water/sea/lakes.</summary>
        public float SeaCoverage { get; set; } = 0.0f;

        /// <summary>How many portals into dungeons this region should expose.</summary>
        public int DungeonPortals { get; set; } = 0;

        public OutdoorConfig(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
