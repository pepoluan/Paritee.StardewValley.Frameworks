﻿using System.IO;
using System.Reflection;

namespace BetterFarmAnimalVariety.Framework.Constants
{
    class Constants
    {
        // Mod
        public const string ModKey = "paritee.betterfarmanimalvariety";
        public const string ConfigFileName = "config.json";
        public const string FarmAnimalsSaveDataFileName = "farmanimals.json";
        public static string ModPath { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        // FarmAnimal
        public const string BabyPrefix = "Baby";
        public const string ShearedPrefix = "Sheared";
        public const int FarmAnimalProduceNone = default(int);

        // AnimalHouse
        public const string Coop = "Coop";
        public const string Barn = "Barn";
        public const string Incubator = "Incubator";
        public const int DefaultIncubatorItem = 101;

        // Content
        public const string ContentPathDelimiter = "\\";
        public const string AnimalsContentDirectory = "Animals";
        public const string DataFarmAnimalsContentDirectory = "Data\\FarmAnimals";
        public const string DataBlueprintsContentDirectory = "Data\\Blueprints";
        public const char DataValueDelimiter = '/';
        public const string None = "none";
        public const int StartingFrame = 0;

        // Event
        public const int SoundInTheNightAnimalEatenEvent = 2;
    }
}