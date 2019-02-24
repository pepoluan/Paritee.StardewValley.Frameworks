﻿using System.Collections.Generic;
using PariteeCore = Paritee.StardewValley.Core;

namespace BetterFarmAnimalVariety.Framework.Models
{
    public class TypeLog
    {
        public string CurrentType;
        public string SavedType;

        public TypeLog(string currentType, string savedType)
        {
            this.CurrentType = currentType;
            this.SavedType = savedType;
        }

        public bool IsDirty()
        {
            return !PariteeCore.Api.FarmAnimal.IsVanilla(this.SavedType);
        }

        public bool IsVanilla()
        {
            return PariteeCore.Api.FarmAnimal.IsVanilla(this.CurrentType);
        }

        public KeyValuePair<string, string> ConvertToKeyValuePair()
        {
            return new KeyValuePair<string, string>(this.CurrentType, this.SavedType);
        }
    }
}
