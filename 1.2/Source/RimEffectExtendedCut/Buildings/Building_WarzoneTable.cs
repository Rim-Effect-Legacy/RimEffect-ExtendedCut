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
    public class Building_WarzoneTable : Building
    {
        private CompPowerTrader powerComp;
        private CompBattleTable compBattleTable;
        public bool CanUse => user == null && powerComp.PowerOn;

        private bool inUse;
        public bool InUse
        {
            get
            {
                var active = user != null && powerComp.PowerOn && user.Spawned && !user.Dead && user.CurJob?.targetA.Thing == this;
                if (inUse != active && !active)
                {
                    inUse = active;
                    this.StopPlay();
                }
                return active;
            }
        }

        private Pawn user;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            compBattleTable = GetComp<CompBattleTable>();
            if (curBattleSet is null)
            {
                curBattleSet = RE_DefOf.RE_WarzoneBattleSet;
            }
        }

        public IntVec3 GetFirstSpot()
        {
            if (compBattleTable is null)
            {
                compBattleTable = GetComp<CompBattleTable>();
            }
            var offfset = compBattleTable.Props.interactionCellOffsets[0];
            IntVec3 b = offfset.RotatedBy(this.Rotation);
            Log.Message("GetFirstSpot: " + (this.Position + b));
            return this.Position + b;
        }

        public IntVec3 GetSecondSpot()
        {
            if (compBattleTable is null)
            {
                compBattleTable = GetComp<CompBattleTable>();
            }
            var offset = compBattleTable.Props.interactionCellOffsets[1];
            IntVec3 b = offset.RotatedBy(this.Rotation);
            Log.Message("GetSecondSpot: " + (this.Position + b));
            return this.Position + b;
        }
        public void StartPlay(Pawn user)
        {
            this.user = user;
            this.inUse = true;
            nextTurnTick = Find.TickManager.TicksGame + curBattleSet.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
        }
        public void StopPlay()
        {
            this.user = null;
            this.inUse = false;
        }

        private int curGameTick;

        public BattleSetDef curBattleSet;

        private int curGameStageInd;

        private Material battleMat;

        private bool dirty;

        private int nextTurnTick;
        public Material BattleMaterial
        {
            get
            {
                if (battleMat is null || dirty)
                {
                    battleMat = MaterialPool.MatFrom(curBattleSet.battleSetTextures[curGameStageInd].texPath);
                    dirty = false;
                }
                return battleMat;
            }
        }
        public override void Draw()
        {
            base.Draw();
            if (InUse)
            {
                var drawPos = this.DrawPos;
                drawPos.y++;

                Vector3 s = new Vector3(this.def.graphicData.drawSize.x, 1f, this.def.graphicData.drawSize.y);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, this.Rotation.AsQuat, s);
                Log.Message("Drawing " + BattleMaterial);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BattleMaterial, 0);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (InUse && Find.TickManager.TicksGame >= nextTurnTick)
            {
                if (curGameStageInd < curBattleSet.battleSetTextures.Count - 1)
                {
                    curGameStageInd++;
                    nextTurnTick = Find.TickManager.TicksGame + curBattleSet.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
                    dirty = true;
                }
                else
                {

                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref curBattleSet, "curBattleSet");
            Scribe_Values.Look(ref curGameTick, "curGameTick");
            Scribe_Values.Look(ref curGameStageInd, "curGameStageInd");
            Scribe_Values.Look(ref nextTurnTick, "nextTurnTick");
        }

        public bool IsGameFinished => curGameStageInd == curBattleSet.battleSetTextures.Count - 1;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Action()
            {
                defaultLabel = "RE.SelectBattleSet".Translate(this.curBattleSet.label),
                defaultDesc = "RE.SelectBattleSetDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/SelectBattleset"),
                action = delegate ()
                {
                    var floatMenu = new FloatMenu(GetBattleSetOptions().ToList());
                    Find.WindowStack.Add(floatMenu);
                }
            };
        }
        private IEnumerable<FloatMenuOption> GetBattleSetOptions()
        {
            foreach (var def in DefDatabase<BattleSetDef>.AllDefs)
            {
                yield return new FloatMenuOption(def.label, delegate()
                {
                    this.curBattleSet = def;
                    this.curGameStageInd = 0;
                }, MenuOptionPriority.Default);
            }
        }

    }
}
