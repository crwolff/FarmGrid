using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FarmGrid.Patches;
using HarmonyLib;
using UnityEngine;

namespace FarmGrid
{
	// Token: 0x02000002 RID: 2
	[BepInPlugin("BepIn.Sarcen.FarmGrid", "FarmGrid", "0.2.1")]
	public class FarmGrid : BaseUnityPlugin
	{
		public static ManualLogSource logger;

		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void Awake()
		{
			logger = Logger;
			FarmGrid.harmony = new Harmony("Harmony.Sarcen.FarmGrid");
			FarmGrid.harmony.PatchAll();
			this.plantSpacing = base.Config.Bind<float>("FarmGrid", "plantSpacing", FarmGrid_Patch.plantSpacing, "Amount of space between plants. This is extra spacing on top of the growthsize needed to grow.");
			this.farmGridSections = base.Config.Bind<int>("FarmGrid", "farmGridSections", FarmGrid_Patch.farmGridSections, "Amount of grid sections (on either side of the main axis.)");
			this.farmGridYOffset = base.Config.Bind<float>("FarmGrid", "farmGridYOffset", FarmGrid_Patch.farmGridYOffset, "Grid offset from the ground.");
			this.farmGridColor = base.Config.Bind<Color>("FarmGrid", "farmGridColor", FarmGrid_Patch.farmGridColor, "Color of the grid.");
			this.plantObjectMasks = base.Config.Bind<string>("FarmGrid", "plantObjectMasks", FarmGrid_Patch.plantObjectMasks.Join(null, ", "), "Masks used by overlapping detection, if you add custom plants that are not in the normal category you may want to add more flags here.\nif you don't know what you are doing, don't touch this.\n\nUse this for Planting Plus:plantObjectMasks = piece,piece_nonsolid,item");
			this.customPlants = base.Config.Bind<string>("FarmGrid", "customPlants", "", "Plants that are not actually plants (objects without a plant component), their name and size.\n\nUse this for Planting Plus:\ncustomPlants = RaspberryBush(Clone): 0.5, RaspberryBush: 0.5, BlueberryBush(Clone): 0.5, BlueberryBush: 0.5, Pickable_Mushroom_yellow(Clone): 0.5, Pickable_Mushroom_yellow: 0.5, Pickable_Mushroom_blue(Clone): 0.5, Pickable_Mushroom_blue: 0.5, CloudberryBush: 0.5, CloudberryBush(Clone): 0.5, Pickable_Thistle(Clone): 0.5, Pickable_Thistle: 0.5, Pickable_Dandelion(Clone): 0.5, Pickable_Dandelion: 0.5");
			FarmGrid_Patch.plantSpacing = this.plantSpacing.Value;
			FarmGrid_Patch.farmGridSections = this.farmGridSections.Value;
			FarmGrid_Patch.farmGridYOffset = this.farmGridYOffset.Value;
			FarmGrid_Patch.farmGridColor = this.farmGridColor.Value;
			string[] array = this.customPlants.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
				float num;
				if (array2.Length == 2 && float.TryParse(array2[1].Trim(), out num))
				{
					FarmGrid_Patch.customPlants.Add(array2[0].Trim(), num);
				}
			}
			string[] array3 = this.plantObjectMasks.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> list = new List<string>();
			foreach (string text in array3)
			{
				list.Add(text.Trim());
			}
			FarmGrid_Patch.plantObjectMasks = list;
		}

		// Token: 0x04000001 RID: 1
		internal const string Author = "Sarcen";

		// Token: 0x04000002 RID: 2
		internal const string Name = "FarmGrid";

		// Token: 0x04000003 RID: 3
		internal const string Version = "0.2.1";

		// Token: 0x04000004 RID: 4
		internal const string BepInGUID = "BepIn.Sarcen.FarmGrid";

		// Token: 0x04000005 RID: 5
		internal const string HarmonyGUID = "Harmony.Sarcen.FarmGrid";

		// Token: 0x04000006 RID: 6
		internal static Harmony harmony;

		// Token: 0x04000007 RID: 7
		private ConfigEntry<float> plantSpacing;

		// Token: 0x04000008 RID: 8
		private ConfigEntry<int> farmGridSections;

		// Token: 0x04000009 RID: 9
		private ConfigEntry<float> farmGridYOffset;

		// Token: 0x0400000A RID: 10
		private ConfigEntry<Color> farmGridColor;

		// Token: 0x0400000B RID: 11
		private ConfigEntry<string> plantObjectMasks;

		// Token: 0x0400000C RID: 12
		private ConfigEntry<string> customPlants;
	}
}
