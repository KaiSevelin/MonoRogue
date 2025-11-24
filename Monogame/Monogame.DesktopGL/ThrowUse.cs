using Microsoft.Xna.Framework;
using RoguelikeMonoGame;
using System;
using System.Collections.Generic;

public sealed class ThrowUse : IUse
{
    public string Id => "Throw";
    public string Label => "Throw";

    public int Damage { get; }
    public int Range { get; }
    public string ProjectileGlyph { get; }
    public bool ConsumesItem { get; }   // e.g., throwing a dagger could remove it from inventory

    public ThrowUse(int damage, int range, string projectileGlyph, bool consumesItem)
    {
        Damage = damage;
        Range = range;
        ProjectileGlyph = projectileGlyph ?? "*";
        ConsumesItem = consumesItem;
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

        var start = new Point(user.Pos.X + dir.X, user.Pos.Y + dir.Y);
        if (!map.InBounds(start) || !map.IsPassable(start))
            return false;


        // Optionally consume the thrown item from the user's "Weapon" slot/inventory
        if (ConsumesItem && user is PlayerCharacter pc)
        {
            var equipped = pc.GetEquipped("Weapon");
            if (equipped != null)
            {
                pc.Unequip("Weapon");
                pc.Inventory.Remove(equipped);
                // (Optional) drop a copy at impact instead of consuming: handle in projectile resolution
            }
        }

        return true;
    }
}
