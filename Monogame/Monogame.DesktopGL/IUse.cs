using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public interface IUse
    {
        string Id { get; }         // eg. "Stab"
        string Label { get; }      // UI label
        int Damage { get; }
        bool Perform(Character user, DungeonMap map, List<NonPlayerCharacter> npcs,
                     Point dir, Random rng);
    }
}
