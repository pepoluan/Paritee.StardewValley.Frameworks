﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterFarmAnimalVariety.Framework.Commands.FarmAnimal
{
    class SetAnimalShopIcon : Command
    {
        public SetAnimalShopIcon(IModHelper helper, IMonitor monitor, ModConfig config)
            : base("bfav_fa_setshopicon", $"Set the category's animal shop icon.\nUsage: bfav_fa_setshopicon <category> <filename>\n- category: the unique animal category.\n- filename: the name of the file (ex. filename.png).", helper, monitor, config) { }

        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        public override void Callback(string command, string[] args)
        {
            try
            {
                Helpers.Assert.GameNotLoaded();
                Helpers.Assert.RequiredArgumentOrder(args.Length, 1, "category");

                string category = args[0].Trim();

                Helpers.Assert.FarmAnimalCategoryExists(category);
                Helpers.Assert.FarmAnimalCanBePurchased(category);
                Helpers.Assert.RequiredArgumentOrder(args.Length, 2, "icon");
                Helpers.Assert.ValidAnimalShopIcon(args[1]);

                Framework.Config.FarmAnimal animal = this.Config.GetCategory(category);

                animal.AnimalShop.Icon = args[1].Trim();

                this.Helper.WriteConfig(this.Config);

                string output = this.DescribeFarmAnimalCategory(animal);

                this.Monitor.Log(output, LogLevel.Info);
            }
            catch (Exception e)
            {
                this.Monitor.Log(e.Message, LogLevel.Error);

                return;
            }
        }
    }
}
