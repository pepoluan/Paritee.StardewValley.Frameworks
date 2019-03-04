﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PariteeCore = Paritee.StardewValley.Core;

namespace BetterFarmAnimalVariety.Framework.Events
{
    class MigrateDeprecatedConfig
    {
        public static void OnEntry(ModEntry mod, string targetFormat, out ModConfig config)
        {
            // Try to load the last supported format...
            string path = Path.Combine(PariteeCore.Constants.Mod.Path, Constants.Mod.ConfigFileName);
            string json = File.ReadAllText(path);
            Config.V2.ModConfig deprecatedConfig = JsonConvert.DeserializeObject<Config.V2.ModConfig>(json);

            //... and migrate them to the current format
            if (!MigrateDeprecatedConfig.ToCurrentFormat<Config.V2.ModConfig>(mod, deprecatedConfig, targetFormat, out config))
            {
                // Escalate the exception if the deprecated config could not be migrated
                throw new FormatException($"Invalid config format. {mod.ModManifest.Version.ToString()} requires format:{mod.ModManifest.Version.MajorVersion.ToString()}.");
            }
        }

        public static bool ToCurrentFormat<T>(ModEntry mod, T deprecatedConfig, string targetFormat, out ModConfig config)
        {
            if (deprecatedConfig is Config.V2.ModConfig)
            {
                MigrateDeprecatedConfig.HandleV2(mod, (Config.V2.ModConfig)Convert.ChangeType(deprecatedConfig, typeof(Config.V2.ModConfig)), targetFormat, out config);

                return true;
            }

            config = new ModConfig();

            return false;
        }

        public static void HandleV2(ModEntry mod, Config.V2.ModConfig deprecatedConfig, string targetFormat, out ModConfig config)
        {
            config = new BetterFarmAnimalVariety.ModConfig
            {
                Format = targetFormat,
                IsEnabled = deprecatedConfig.IsEnabled
            };

            MigrateDeprecatedConfig.CreateContentPack(mod, deprecatedConfig);
        }

        public static bool CreateContentPack(ModEntry mod, Config.V2.ModConfig deprecatedConfig)
        {
            string voidChicken = PariteeCore.Constants.VanillaFarmAnimalType.VoidChicken.ToString();

            // Get ready to make a new content pack
            ContentPacks.FarmAnimals farmAnimals = new ContentPacks.FarmAnimals(new List<ContentPacks.FarmAnimalCategory>());

            List<string> iconsToBeMoved = new List<string>();

            // Go through all of the old categories so that we can determine if we need to make a new content pack to preserve the changes
            foreach (KeyValuePair<string, Config.V2.ConfigFarmAnimal> oldFarmAnimals in deprecatedConfig.FarmAnimals)
            {
                // Check if this category is a vanila category
                bool isVanilla = PariteeCore.Constants.VanillaFarmAnimalCategory.All()
                    .Exists(o => o.ToString() == oldFarmAnimals.Key);

                // Always create the category with the update for vanilla and create for non-vanilla
                ContentPacks.FarmAnimalCategory.Actions action = isVanilla ? ContentPacks.FarmAnimalCategory.Actions.Update : ContentPacks.FarmAnimalCategory.Actions.Create;

                Cache.FarmAnimalStock animalShop = null;
                bool forceOverrideRemoveFromShop = true;
                bool forceOverrideExclude = false;

                if (oldFarmAnimals.Value.CanBePurchased())
                {
                    Int32.TryParse(oldFarmAnimals.Value.AnimalShop.Price, out int price);

                    List<string> exclude = new List<string>();

                    if (!deprecatedConfig.IsChickenCategory(oldFarmAnimals.Key) || (oldFarmAnimals.Value.Types.Contains(voidChicken) && deprecatedConfig.AreVoidFarmAnimalsInShopAlways()))
                    {
                        exclude.Add(voidChicken);
                    }

                    // Check to make sure the icon exists...
                    string icon = oldFarmAnimals.Value.AnimalShop.Icon;
                    string pathToIcon = Path.Combine(PariteeCore.Constants.Mod.Path, icon);

                    if (File.Exists(pathToIcon))
                    {
                        iconsToBeMoved.Add(icon);
                    }
                    else
                    {
                        icon = null;
                    }

                    animalShop = new Cache.FarmAnimalStock
                    {
                        Name = oldFarmAnimals.Value.AnimalShop.Name,
                        Description = oldFarmAnimals.Value.AnimalShop.Description,
                        Icon = icon,
                        Price = Math.Abs(price),
                        Exclude = exclude
                    };

                    forceOverrideRemoveFromShop = false;
                    forceOverrideExclude = true;
                }

                // Create the start of the category
                ContentPacks.FarmAnimalCategory category = new ContentPacks.FarmAnimalCategory(action)
                {
                    Category = oldFarmAnimals.Key,
                    Types = oldFarmAnimals.Value.Types.Select(str => new Cache.FarmAnimalType(str)).ToList(),
                    Buildings = oldFarmAnimals.Value.Buildings.ToList(),
                    AnimalShop = animalShop,
                    ForceOverrideTypes = true,
                    ForceOverrideBuildings = true,
                    ForceRemoveFromShop = forceOverrideRemoveFromShop,
                    ForceOverrideExclude = forceOverrideExclude,
                };

                farmAnimals.Categories.Add(category);
            }

            if (!farmAnimals.Categories.Any())
            {
                return false;
            }

            string contentPackDirectoryPath = Constants.Mod.ConfigMigrationContentPackFullPath;
            string contentPackId = Guid.NewGuid().ToString("N");

            DirectoryInfo dir = new System.IO.DirectoryInfo(contentPackDirectoryPath);
            dir.Create(); // If the directory already exists, this method does nothing.

            // content.json
            string contentJsonFilePath = Path.Combine(contentPackDirectoryPath, Constants.Mod.ContentPackContentFileName);
            string contentJson = JsonConvert.SerializeObject(farmAnimals, Formatting.Indented, new JsonSerializerSettings{ NullValueHandling = NullValueHandling.Ignore });

            // manifest.json
            JObject manifest = JObject.FromObject(new
            {
                Name = Constants.Mod.ConfigMigrationContentPackName,
                Author = Constants.Mod.ConfigMigrationContentPackAuthor,
                Version = Constants.Mod.ConfigMigrationContentPackVersion,
                Description = Constants.Mod.ConfigMigrationContentPackDescription,
                UniqueID = contentPackId,
                ContentPackFor = new
                {
                    UniqueID = Constants.Mod.Key
                },
            });

            string manifestJsonFilePath = Path.Combine(contentPackDirectoryPath, Constants.Mod.ContentPackManifestFileName);
            string manifestJson = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            // Write the files
            File.WriteAllText(manifestJsonFilePath, manifestJson);
            File.WriteAllText(contentJsonFilePath, contentJson);

            foreach (string sourceFileName in iconsToBeMoved)
            {
                string destFileName = Path.Combine(contentPackDirectoryPath, sourceFileName);

                // Create the subfolders if necessary
                System.IO.FileInfo destinationDirectory = new System.IO.FileInfo(destFileName);
                destinationDirectory.Directory.Create();

                // Copy the icon to the new location
                File.Copy(Path.Combine(PariteeCore.Constants.Mod.Path, sourceFileName), destFileName);
            }

            // Update the cache
            IContentPack contentPack = mod.Helper.ContentPacks.CreateTemporary(
               directoryPath: contentPackDirectoryPath,
               id: contentPackId,
               name: Constants.Mod.ConfigMigrationContentPackName,
               description: Constants.Mod.ConfigMigrationContentPackDescription,
               author: Constants.Mod.ConfigMigrationContentPackAuthor,
               version: new SemanticVersion(Constants.Mod.ConfigMigrationContentPackVersion)
            );

            LoadContentPacks.SetUpContentPacks(new List<IContentPack>() { contentPack }, mod.Monitor);

            return true;
        }
    }
}
