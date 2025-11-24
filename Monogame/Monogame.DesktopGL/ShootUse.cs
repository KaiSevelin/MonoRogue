using Microsoft.Xna.Framework;
using RoguelikeMonoGame;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ShootUse : IUse
{
    public string Id => "Shoot";
    public string Label => "Shoot";

    public int Damage { get; }
    public int Range { get; }
    public string ProjectileGlyph { get; }
    public string AmmoItemId { get; }    // "" or null means no ammo required

    public ShootUse(int damage, int range, string projectileGlyph, string ammoItemId = "")
    {
        Damage = damage;
        Range = range;
        ProjectileGlyph = projectileGlyph ?? "*";
        AmmoItemId = ammoItemId ?? "";
    }

    public bool Perform(
        Character user,
        DungeonMap map,
        List<NonPlayerCharacter> npcs,
        Point dir,
        Random rng)
    {
        if (dir == Point.Zero) dir = user.Facing;
        if (dir == Point.Zero) return false;

        // Optional ammo check/consume (player only)
        if (!string.IsNullOrEmpty(AmmoItemId) && user is PlayerCharacter pc)
        {
            if (!TryConsumeAmmo(pc, AmmoItemId))
                return false; // no ammo -> no shot
        }

        var start = new Point(user.Pos.X + dir.X, user.Pos.Y + dir.Y);
        if (!map.InBounds(start) || !map.IsPassable(start))
            return false;

        return true;
    }

    private static bool TryConsumeAmmo(PlayerCharacter pc, string ammoId)
    {
        // Assumes ammo items are DataBackedItem with DefId == ammoId
        var idx = pc.Inventory.FindIndex(i => i is DataBackedItem d && d.DefId == ammoId);
        if (idx < 0) return false;
        pc.Inventory.RemoveAt(idx);
        return true;
    }
}
