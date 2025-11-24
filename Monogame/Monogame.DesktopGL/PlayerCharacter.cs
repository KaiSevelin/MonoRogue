using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RogueTest;

namespace RoguelikeMonoGame
{
    public sealed class PlayerCharacter : Character
    {
        // Simple key counter for locked/secret doors
        public int Keys { get; set; } = 0;

        // Slot-based equipment (e.g., "Weapon", "Light", "Offhand", "Armor"...)
        public Dictionary<string, Item> Equipped { get; private set; } = new();

        // Convenience accessors
        public Item GetEquipped(string slot) => Equipped.TryGetValue(slot, out var it) ? it : null;
        public WeaponItem? EquippedWeapon => GetEquipped("Weapon") as WeaponItem;
        protected override IEnumerable<Item> GetEquippedItems() => Equipped.Values;

        public PlayerCharacter(Point pos) : base(pos)
        {
            MaxHP = 200;
            HP = 200;
            // Inventory is defined on Character; initialize if needed
            Inventory ??= new List<Item>();
        }
        public bool TryRevealSecretDoor(DungeonMap map, Point p)
        {
            if (!map.InBounds(p)) return false;

            if (map.Doors.TryGetValue(p, out var door) && door is SecretDoorObject secret && !secret.Discovered)
            {
                secret.Reveal(map, this);
                return true;
            }

            return false;
        }
        // ===== Equipment =====
        public void Equip(Item item, string slot)
        {
            if (item == null) return;
            if (Inventory == null || !Inventory.Contains(item)) return;

            if (Equipped.TryGetValue(slot, out var old))
                old.OnUnequip(this);

            Equipped[slot] = item;
            item.OnEquip(this);
        }

        public void Unequip(string slot)
        {
            if (Equipped.TryGetValue(slot, out var it))
            {
                it.OnUnequip(this);
                Equipped.Remove(slot);
            }
        }

        public void UnequipAll()
        {
            foreach (var it in Equipped.Values)
                it.OnUnequip(this);
            Equipped.Clear();
        }

        // ===== Movement / Bump-to-attack =====
        // Attempt to step; if an enemy occupies the target, use melee
        public void TryMove(Point dir, DungeonMap map, World world, List<NonPlayerCharacter> npcs,
                             Random rng)
        {
            if (dir == Point.Zero) return;
            SetFacing(dir);

            var next = new Point(Pos.X + dir.X, Pos.Y + dir.Y);
            if (!map.InBounds(next)) return;

            // If blocked by terrain, stop
            if (!map.IsPassable(next))
                return;

            // If an NPC is there, try melee uses (Slash/Stab), don't move
            var enemy = npcs.Find(n => n.Pos == next);
            if (enemy != null)
            {
                if (EquippedWeapon != null)
                {
                    if (EquippedWeapon.PerformUse("Slash", this, map, npcs,  dir, rng)) return;
                    if (EquippedWeapon.PerformUse("Stab", this, map, npcs,  dir, rng)) return;
                }

                // bare-hands fallback
                enemy.TakeDamage(1);
                return;
            }

            // Otherwise, move
            Pos = next;
            world.HandleStepOnTraps(this, map, npcs);
        }
        public bool Search(DungeonMap map, int radius)
        {
            bool revealedSomething = false;

            for (int y = Pos.Y - radius; y <= Pos.Y + radius; y++)
            {
                for (int x = Pos.X - radius; x <= Pos.X + radius; x++)
                {
                    var p = new Point(x, y);
                    if (!map.InBounds(p)) continue;

                    // Optional: restrict to tiles you've already explored
                    // if (!Visible[x, y] && !Explored[x, y]) continue;

                    int dx = x - Pos.X;
                    int dy = y - Pos.Y;
                    // circular radius; use manhattan if you prefer
                    if (dx * dx + dy * dy > radius * radius) continue;

                    // Any items at this tile?
                    if (map.ItemsAt.TryGetValue(p, out var items))
                    {
                        foreach (var it in items)
                        {
                            if (it is IHiddenRevealable hidden && !hidden.Discovered)
                            {
                                hidden.Reveal(map, this);
                                revealedSomething = true;
                            }
                        }
                    }

                    // Doors tracked separately in map.Doors
                    if (map.Doors.TryGetValue(p, out var door) &&
                        door is IHiddenRevealable hiddenDoor &&
                        !hiddenDoor.Discovered)
                    {
                        hiddenDoor.Reveal(map, this);
                        revealedSomething = true;
                    }
                }
            }

            return revealedSomething;
        }

        // ===== Primary action (ranged-first, then melee) =====
        public bool PrimaryAttack(DungeonMap map, List<NonPlayerCharacter> npcs,
                                   Random rng)
        {
            var dir = Facing;
            if (dir == Point.Zero) dir = new Point(1, 0);

            if (EquippedWeapon != null)
            {
                // Ranged first (Shoot/Throw)
                if (EquippedWeapon.PerformUse("Shoot", this, map, npcs, dir, rng)) return true;
                if (EquippedWeapon.PerformUse("Throw", this, map, npcs, dir, rng)) return true;

                // Then melee forward (Slash/Stab)
                if (EquippedWeapon.PerformUse("Slash", this, map, npcs, dir, rng)) return true;
                if (EquippedWeapon.PerformUse("Stab", this, map, npcs, dir, rng)) return true;
            }

            // Bare-hands melee fallback
            var tgt = new Point(Pos.X + dir.X, Pos.Y + dir.Y);
            var e = npcs.Find(o => o.Pos == tgt);
            if (e != null) { e.TakeDamage(1); return true; }

            return false;
        }

        // ===== GameObject abstract =====
        // The player doesn't "interact with the player"; keep no-op.
        public override void Interact(PlayerCharacter player, DungeonMap map,
                                      List<NonPlayerCharacter> npcs,
                                      Dictionary<Point, List<Item>> itemsAt)
        {
            // No-op; interactions are initiated from Game1 targeting other GameObjects.
        }
    }
}
