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
