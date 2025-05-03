using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaFurnitureExpanded;
using Verse;

namespace RimEffectExtendedCut
{
	public class CompProperties_OutDoorLamp : CompProperties_Power
	{
		public float storedEnergyMax = 1000f;

		public float efficiency = 0.5f;

		public float selfCharging = 30f;

		public float maxSolarPowerGain = 300f;
		public CompProperties_OutDoorLamp()
        {
			compClass = typeof(CompPowerOutDoorLamp);
        }
	}
	public class CompPowerOutDoorLamp : ThingComp
	{
		private float SolarPower => Mathf.Lerp(0f, Props.maxSolarPowerGain, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;
		protected float DesiredPowerOutput => SolarPower;
		private float RoofedPowerOutputFactor
		{
			get
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 item in parent.OccupiedRect())
				{
					num++;
					if (parent.Map.roofGrid.Roofed(item))
					{
						num2++;
					}
				}
				return (float)(num - num2) / (float)num;
			}
		}

		private float storedEnergy;
		public float AmountCanAccept
		{
			get
			{
				if (parent.IsBrokenDown())
				{
					return 0f;
				}
				CompProperties_OutDoorLamp props = Props;
				return (props.storedEnergyMax - storedEnergy) / props.efficiency;
			}
		}

		public float StoredEnergy => storedEnergy;

		public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

		public CompProperties_OutDoorLamp Props => (CompProperties_OutDoorLamp)props;

		private CompGlowerExtended compGlowerExtended;
		public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			compGlowerExtended = this.parent.GetComp<CompGlowerExtended>();
		}
        public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref storedEnergy, "storedPower", 0f);
			CompProperties_OutDoorLamp props = Props;
			if (storedEnergy > props.storedEnergyMax)
			{
				storedEnergy = props.storedEnergyMax;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			var gainedEnergy = DesiredPowerOutput;
			AddEnergy(gainedEnergy * CompPower.WattsToWattDaysPerTick);
			if (compGlowerExtended.compGlower != null)
            {
				DrawPower(Mathf.Min(Props.selfCharging * CompPower.WattsToWattDaysPerTick, storedEnergy));
            }
			if (compGlowerExtended != null)
            {
				var localHour = GenLocalDate.HourOfDay(this.parent.Map);
				if (localHour >= 8 && localHour <= 19)
                {
					if (compGlowerExtended.compGlower != null)
					{
						compGlowerExtended.RemoveGlower(this.parent.Map);
					}
				}
				else
                {
					if (storedEnergy <= 0)
					{
						if (compGlowerExtended.compGlower != null)
						{
							compGlowerExtended.RemoveGlower(this.parent.Map);
						}
					}
					else if (compGlowerExtended.compGlower == null)
					{
						compGlowerExtended.UpdateGlower(compGlowerExtended.currentColorInd);
					}
				}
			}

		}
		public void AddEnergy(float amount)
		{
			if (amount < 0f)
			{
				Log.Error("Cannot add negative energy " + amount);
				return;
			}
			if (amount > AmountCanAccept)
			{
				amount = AmountCanAccept;
			}
			amount *= Props.efficiency;
			storedEnergy += amount;
		}

		public void DrawPower(float amount)
		{
			storedEnergy -= amount;
			if (storedEnergy < 0f)
			{
				Log.Error("Drawing power we don't have from " + parent);
				storedEnergy = 0f;
			}
		}

		public void SetStoredEnergyPct(float pct)
		{
			pct = Mathf.Clamp01(pct);
			storedEnergy = Props.storedEnergyMax * pct;
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "Breakdown")
			{
				DrawPower(StoredEnergy);
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_OutDoorLamp props = Props;
			string t = "PowerBatteryStored".Translate() + ": " + storedEnergy.ToString("F0") + " / " + props.storedEnergyMax.ToString("F0") + " Wd";
			t += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (props.efficiency * 100f).ToString("F0") + "%";
			if (storedEnergy > 0f)
			{
				t += "\n" + "SelfDischarging".Translate() + ": " + Props.selfCharging.ToString("F0") + " W";
			}
			return t;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Prefs.DevMode)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEBUG: Fill";
				command_Action.action = delegate
				{
					SetStoredEnergyPct(1f);
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEBUG: Empty";
				command_Action2.action = delegate
				{
					SetStoredEnergyPct(0f);
				};
				yield return command_Action2;
			}
		}
	}
}
