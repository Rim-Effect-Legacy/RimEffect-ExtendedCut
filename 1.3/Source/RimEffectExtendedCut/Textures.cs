using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimEffectExtendedCut
{
	[StaticConstructorOnStartup]
	public static class Textures
	{
		public static readonly Material InteractionCellMaterial = MaterialPool.MatFrom("UI/Overlays/InteractionCell", ShaderDatabase.Transparent);

		public static readonly Color InteractionCellIntensity = new Color(1f, 1f, 1f, 0.3f);
	}
}
