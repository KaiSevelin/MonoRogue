using Microsoft.Xna.Framework;
using RogueTest;

namespace RoguelikeMonoGame
{
    public static class ItemFactory
    {
        public static Item CreateLight(string id, Point pos)
            => new LightSourceItem(pos, ItemRegistry.Lights[id]);

        public static Item CreateLightInInventory(string id)
            => new LightSourceItem(ItemRegistry.Lights[id]);

        public static Item CreateWeapon(string id, Point pos)
            => new WeaponItem(pos, ItemRegistry.Weapons[id]);

        public static Item CreateWeaponInInventory(string id)
            => new WeaponItem(ItemRegistry.Weapons[id]);
    }
}
