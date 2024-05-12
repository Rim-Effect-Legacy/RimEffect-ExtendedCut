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
	[StaticConstructorOnStartup]
	public class Bullet_ChakramLauncher : Projectile_Explosive
	{

		private new float ArcHeightFactor
		{
			get
			{
				float num = def.projectile.arcHeightFactor;
				float num2 = (destination - origin).MagnitudeHorizontalSquared();
				if (num * num > num2 * 0.2f * 0.2f)
				{
					num = Mathf.Sqrt(num2) * 0.2f;
				}
				return num;
			}
		}

		private Material groundMat;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
			GraphicRequest req = new GraphicRequest(this.def.graphicData.graphicClass, this.def.graphicData.texPath + "_Ground", this.def.graphicData.shaderType.Shader, 
				this.def.graphicData.drawSize, this.def.graphicData.color, this.def.graphicData.colorTwo, this.def.graphicData, 0, null, null);

			MaterialRequest req2 = default(MaterialRequest);
			req2.mainTex = ContentFinder<Texture2D>.Get(req.path);
			req2.shader = req.shader;
			req2.color = req.color;
			req2.colorTwo = req.colorTwo;
			req2.renderQueue = req.renderQueue;
			req2.shaderParameters = req.shaderParameters;
			groundMat = MaterialPool.MatFrom(req2);
		}
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
			float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
			Vector3 drawPos = DrawPos;
			Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
			if (def.projectile.shadowSize > 0f)
			{
				DrawShadow(drawPos, num);
			}
			if (onGround)
            {
				Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), position, ExactRotation, groundMat, 0);
			}
			else
            {
				Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), position, ExactRotation, def.DrawMatSingle, 0);
            }
			Comps_PostDraw();
		}
		private static new readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);

		private new void DrawShadow(Vector3 drawLoc, float height)
		{
			if (!(shadowMaterial == null))
			{
				float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
				Vector3 s = new Vector3(num, 1f, num);
				Vector3 b = new Vector3(0f, -0.01f, 0f);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawLoc + b, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
			}
		}
		public override void Tick()
        {
            base.Tick();
			if (this.Map != null && onGround)
            {
				var pawns = this.Position.GetThingList(this.Map).OfType<Pawn>().ToList();
				for (int num = pawns.Count - 1; num >= 0; num--)
				{
					pawns[num].TryAttachFire(Rand.Range(0.3f, 0.6f), null);
				}
			}

        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
            onGround = true;
        }

		private bool onGround;
        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref onGround, "onGround");
        }
    }
}
