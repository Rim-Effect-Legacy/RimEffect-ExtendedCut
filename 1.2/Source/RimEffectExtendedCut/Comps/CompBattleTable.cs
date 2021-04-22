using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimEffectExtendedCut
{
	public class CompProperties_BattleTable : CompProperties_Power
	{
        public List<IntVec3> interactionCellOffsets;

		public CompProperties_BattleTable()
        {
			compClass = typeof(CompBattleTable);
        }
	}
	public class CompBattleTable : ThingComp
	{
		public CompProperties_BattleTable Props => this.props as CompProperties_BattleTable;
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
			if (this.Props.interactionCellOffsets != null)
            {
				DrawInteractionCell(this.parent.def, this.parent.Position, this.parent.Rotation);
            }
        }
        public void DrawInteractionCell(ThingDef tDef, IntVec3 center, Rot4 placingRot)
		{
			foreach (var interactionSpot in this.Props.interactionCellOffsets)
            {
				IntVec3 c = InteractionCellWhenAt(interactionSpot, center, placingRot);
				Vector3 vector = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
				if (tDef.interactionCellGraphic == null && tDef.interactionCellIcon != null)
				{
					ThingDef thingDef = tDef.interactionCellIcon;
					if (thingDef.blueprintDef != null)
					{
						thingDef = thingDef.blueprintDef;
					}
					tDef.interactionCellGraphic = thingDef.graphic.GetColoredVersion(ShaderTypeDefOf.EdgeDetect.Shader, Textures.InteractionCellIntensity, Color.white);
				}
				if (tDef.interactionCellGraphic != null)
				{
					Rot4 rot = tDef.interactionCellIconReverse ? placingRot.Opposite : placingRot;
					tDef.interactionCellGraphic.DrawFromDef(vector, rot, tDef.interactionCellIcon);
				}
				else
				{
					Graphics.DrawMesh(MeshPool.plane10, vector, Quaternion.identity, Textures.InteractionCellMaterial, 0);
				}
			}
		}

		public IntVec3 InteractionCellWhenAt(IntVec3 interactionCellOffset, IntVec3 center, Rot4 rot)
		{
			IntVec3 b = interactionCellOffset.RotatedBy(rot);
			return center + b;
		}
	}
}
