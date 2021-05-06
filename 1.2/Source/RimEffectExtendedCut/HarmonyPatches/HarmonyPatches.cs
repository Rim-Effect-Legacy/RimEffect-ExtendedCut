using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimEffectExtendedCut
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			Harmony harmony = new Harmony("Helixien.RimEffectExtendedCut");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(Building), "Destroy")]
		private static class Patch_BuildingDestroy
		{
			private static void Prefix(Building __instance)
			{
				if (__instance != null && __instance.def != null && __instance.def.passability == Traversability.Impassable && __instance.Map != null)
				{
					foreach (var t in __instance.Position.GetThingList(__instance.Map).OfType<Building_WallLight>().Where(b => b != __instance))
                    {
						 t.Destroy(DestroyMode.Refund);
                    }
				}
			}
		}

		[HarmonyPatch(typeof(WatchBuildingUtility), "GetWatchCellRect")]
		private static class Patch_GetWatchCellRect
		{
			private static bool Prefix(ref CellRect __result, ThingDef def, IntVec3 center, Rot4 rot, int watchRot)
			{
				if (def == RE_DefOf.RE_HolovisionTable)
				{
					__result = GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(2);
					return false;
				}
				return true;
			}
		}
	}
}
