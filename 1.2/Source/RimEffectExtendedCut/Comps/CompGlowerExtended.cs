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
    public class ColorModExtension : DefModExtension
    {
        public List<ColorOption> colorOptions = new List<ColorOption>();
    }
    public class ColorOption
    {
		public float overlightRadius;

		public float glowRadius = 14f;

        public string texPath;

		public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 1.45f;

		public string colorLabel = "";
    }
	public class CompProperties_GlowerExtended : CompProperties
    {
        public List<ColorOption> colorOptions;

        public bool spawnGlowerInFacedCell;
		public CompProperties_GlowerExtended()
		{
			compClass = typeof(CompGlowerExtended);
		}
	}

	public class CompGlowerExtended : ThingComp
	{
		private ColorOption currentColor;
		public int currentColorInd;
        public CompGlower compGlower;
        private bool dirty;
        private CompPowerTrader compPower;
        public CompProperties_GlowerExtended Props => (CompProperties_GlowerExtended)props;
        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + " (" + currentColor.colorLabel + ")";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			this.currentColor = Props.colorOptions[currentColorInd];
            this.dirty = true;
            this.compPower = this.parent.GetComp<CompPowerTrader>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (dirty)
            {
                if (compPower == null || compPower.PowerOn)
                {
                    this.UpdateGlower(currentColorInd);
                    this.ChangeGraphic();
                }
                dirty = false;
            }
            if (compPower != null)
            {
                if (compPower.PowerOn && this.compGlower == null)
                {
                    this.UpdateGlower(this.currentColorInd);
                }
                else if (!compPower.PowerOn && this.compGlower != null)
                {
                    this.RemoveGlower();
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.parent.Faction == Faction.OfPlayer)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.disabled = compPower != null ? !compPower.PowerOn : false;
                command_Action.action = delegate
                {
                    if (compPower != null)
                    {
                        if (compPower.PowerOn)
                        {
                            SwitchColor();
                        }
                    }
                    else
                    {
                        SwitchColor();
                    }
                };
                command_Action.defaultLabel = "RE.SwitchLightColor".Translate();
                command_Action.defaultDesc = "RE.SwitchLightColorDesc".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Icons/LampColourSwitch");
                yield return command_Action;
            }
        }

        private void SwitchColor()
        {
            if (this.currentColorInd == Props.colorOptions.Count - 1)
            {
                this.UpdateGlower(0);
                this.ChangeGraphic();
            }
            else
            {
                this.UpdateGlower(this.currentColorInd + 1);
                this.ChangeGraphic();
            }
        }
        public void RemoveGlower()
        {
            if (this.compGlower != null)
            {
                base.parent.Map.glowGrid.DeRegisterGlower(this.compGlower);
                this.compGlower = null;
            }
        }
        public void UpdateGlower(int colorOptionInd)
        {
            Log.Message(" - UpdateGlower - RemoveGlower(); - 1", true);
            RemoveGlower();
            Log.Message(" - UpdateGlower - var colorOption = Props.colorOptions[colorOptionInd]; - 2", true);
            var colorOption = Props.colorOptions[colorOptionInd];
            Log.Message(" - UpdateGlower - this.currentColor = colorOption; - 3", true);
            this.currentColor = colorOption;
            Log.Message(" - UpdateGlower - this.currentColorInd = colorOptionInd; - 4", true);
            this.currentColorInd = colorOptionInd;
            Log.Message(" - UpdateGlower - this.compGlower = new CompGlower(); - 5", true);
            this.compGlower = new CompGlower();
            Log.Message(" - UpdateGlower - Thing dummyThing = null; - 6", true);
            Thing dummyThing = null;
            Log.Message(" - UpdateGlower - if (Props.spawnGlowerInFacedCell) - 7", true);
            if (Props.spawnGlowerInFacedCell)
            {
                Log.Message(" - UpdateGlower - dummyThing = ThingMaker.MakeThing(ThingDef.Named(\"RE_WallLightDummy\")); - 8", true);
                dummyThing = ThingMaker.MakeThing(ThingDef.Named("RE_WallLightDummy"));
                Log.Message(" - UpdateGlower - var cellGlower = this.parent.Position + base.parent.Rotation.FacingCell; - 9", true);
                var cellGlower = this.parent.Position + base.parent.Rotation.FacingCell;
                Log.Message(" - UpdateGlower - GenSpawn.Spawn(dummyThing, cellGlower, this.parent.Map); - 10", true);
                GenSpawn.Spawn(dummyThing, cellGlower, this.parent.Map);
                Log.Message(" - UpdateGlower - this.compGlower.parent = dummyThing as ThingWithComps; - 11", true);
                this.compGlower.parent = dummyThing as ThingWithComps;
            }
            else
            {
                Log.Message(" - UpdateGlower - this.compGlower.parent = this.parent; - 12", true);
                this.compGlower.parent = this.parent;
            }
            this.compGlower.Initialize(new CompProperties_Glower()
            {
                glowColor = colorOption.glowColor,
                glowRadius = colorOption.glowRadius,
                overlightRadius = colorOption.overlightRadius
            });
            Log.Message(" - UpdateGlower - base.parent.Map.mapDrawer.MapMeshDirty(base.parent.Position, MapMeshFlag.Things); - 14", true);
            base.parent.Map.mapDrawer.MapMeshDirty(base.parent.Position, MapMeshFlag.Things);
            Log.Message(" - UpdateGlower - base.parent.Map.glowGrid.RegisterGlower(this.compGlower); - 15", true);
            base.parent.Map.glowGrid.RegisterGlower(this.compGlower);
            Log.Message(" - UpdateGlower - if (Props.spawnGlowerInFacedCell) - 16", true);
            if (Props.spawnGlowerInFacedCell)
            {
                Log.Message(" - UpdateGlower - dummyThing.DeSpawn(); - 17", true);
                dummyThing.DeSpawn();
            }
        }


        public void ChangeGraphic()
        {
            Log.Message(" - ChangeGraphic - if (!this.currentColor.texPath.NullOrEmpty()) - 1", true);
            if (!this.currentColor.texPath.NullOrEmpty())
            {

                var graphicData = new GraphicData();
                Log.Message(" - ChangeGraphic - graphicData.graphicClass = this.parent.def.graphicData.graphicClass; - 3", true);
                graphicData.graphicClass = this.parent.def.graphicData.graphicClass;
                Log.Message(" - ChangeGraphic - graphicData.texPath = this.currentColor.texPath; - 4", true);
                graphicData.texPath = this.currentColor.texPath;
                Log.Message(" - ChangeGraphic - graphicData.shaderType = this.parent.def.graphicData.shaderType; - 5", true);
                graphicData.shaderType = this.parent.def.graphicData.shaderType;
                Log.Message(" - ChangeGraphic - graphicData.drawSize = this.parent.def.graphicData.drawSize; - 6", true);
                graphicData.drawSize = this.parent.def.graphicData.drawSize;
                Log.Message(" - ChangeGraphic - graphicData.color = this.parent.def.graphicData.color; - 7", true);
                graphicData.color = this.parent.def.graphicData.color;
                Log.Message(" - ChangeGraphic - graphicData.colorTwo = this.parent.def.graphicData.colorTwo; - 8", true);
                graphicData.colorTwo = this.parent.def.graphicData.colorTwo;

                var newGraphic = graphicData.GraphicColoredFor(this.parent);
                Log.Message(" - ChangeGraphic - Traverse.Create(this.parent).Field(\"graphicInt\").SetValue(newGraphic); - 10", true);
                Traverse.Create(this.parent).Field("graphicInt").SetValue(newGraphic);
                Log.Message(" - ChangeGraphic - base.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things); - 11", true);
                base.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref currentColorInd, "currentColorInd");
        }
    }
}
