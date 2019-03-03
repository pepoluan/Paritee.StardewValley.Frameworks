﻿using Harmony;

namespace BetterFarmAnimalVariety.Framework.Patches.PurchaseAnimalsMenu
{
    [HarmonyPatch(typeof(StardewValley.Menus.PurchaseAnimalsMenu), "getAnimalDescription")]
    class GetAnimalDescription : Patch
    {
        public static bool Prefix(ref string name, ref string __result)
        {
            // Get the description from the config
            string category = name;

            __result = Helpers.FarmAnimals.GetCategory(category).Category.AnimalShop.Description;

            return false;
        }
    }
}
