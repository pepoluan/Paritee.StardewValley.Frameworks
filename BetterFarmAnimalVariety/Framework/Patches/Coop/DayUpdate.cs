﻿using Harmony;

namespace BetterFarmAnimalVariety.Framework.Patches.Coop
{
    [HarmonyPatch(typeof(StardewValley.Buildings.Coop), "dayUpdate")]
    class DayUpdate
    {
        public static bool Prefix(ref StardewValley.Buildings.Coop __instance, ref int dayOfMonth)
        {
            StardewValley.AnimalHouse animalHouse = Api.AnimalHouse.GetIndoors(__instance);

            if (!Api.AnimalHouse.IsEggReadyToHatch(animalHouse))
            {
                // Will fail same checks which is what we want
                return true;
            }

            // WARNING:
            // Original code adds an animal at this time to the coop. This causes 
            // the animals to be prematurely added to the saves when they should 
            // only be added after a successful naming event. This diverges significantly
            // from the vanilla code.
            Api.AnimalHouse.ResetIncubator(animalHouse);
            //animalHouse.map.GetLayer("Front").Tiles[1, 2].TileIndex = 45; // TODO: check what this does - egg in incubator graphic?

            // Always want to continue because setting the X and Y values will 
            // guarantee that it won't trigger the hatch again
            return true;
        }
    }
}