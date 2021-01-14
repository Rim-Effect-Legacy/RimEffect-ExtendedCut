using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimEffectExtendedCut
{
	public class PlaceWorker_OnWall : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			AcceptanceReport result = false;
			var edifice = loc.GetEdifice(map);
			if (loc.InBounds(map) && (edifice?.def.defName.ToLower().Contains("wall") ?? false))
			{
				result = true;
			}
			return result;
		}
	}
}
