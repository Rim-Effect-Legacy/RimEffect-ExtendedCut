using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimEffectExtendedCut
{
    public class Building_WarzoneTable : Building
    {
        private CompPowerTrader powerComp;
        private CompBattleTable compBattleTable;
        public bool CanUse => user == null && powerComp.PowerOn;

        private bool inUse;

        private int curGameTick;

        public BattleSetDef curBattleSetDef;

        private int curGameStageInd;

        private Material battleMat;

        private bool dirty;

        private int nextTurnTick;

        private BattleSetStep winningBattleSet;

        private bool winnerIsDetermined;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref curBattleSetDef, "curBattleSetDef");
            Scribe_Values.Look(ref curGameTick, "curGameTick");
            Scribe_Values.Look(ref curGameStageInd, "curGameStageInd");
            Scribe_Values.Look(ref nextTurnTick, "nextTurnTick");
            Scribe_Values.Look(ref winnerIsDetermined, "winnerIsDetermined");
            Scribe_Deep.Look(ref winningBattleSet, "winningBattleSet");
        }

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
            if (curBattleSetDef is null)
            {
                curBattleSetDef = RE_DefOf.RE_FirstContactWarBattleSet;
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
            return this.Position + b;
        }
        public void StartPlay(Pawn user)
        {
            this.user = user;
            this.inUse = true;
            curGameStageInd = 0;
            nextTurnTick = Find.TickManager.TicksGame + curBattleSetDef.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
            winnerIsDetermined = false;
            winningBattleSet = null;
            battleMat = null;
            dirty = true;
        }
        public void StopPlay()
        {
            this.user = null;
            this.inUse = false;
            curGameStageInd = 0;
        }

        public void SelectWinner(Pawn winner, Pawn loser)
        {
            if (winner == user)
            {
                winningBattleSet = this.curBattleSetDef.winningTextures.FirstOrDefault(x => x.playerA == BattleCondition.Win);
            }
            else
            {
                winningBattleSet = this.curBattleSetDef.winningTextures.FirstOrDefault(x => x.playerB == BattleCondition.Win);
            }
            nextTurnTick = Find.TickManager.TicksGame + winningBattleSet.ticksInterval.RandomInRange;
            dirty = true;
            winnerIsDetermined = true;
            if (winningBattleSet.playerWonThought != null)
            {
                winner.needs?.mood?.thoughts?.memories?.TryGainMemory(winningBattleSet.playerWonThought);
            }
            if (winningBattleSet.playerLoseThought != null)
            {
                loser.needs?.mood?.thoughts?.memories?.TryGainMemory(winningBattleSet.playerWonThought);
            }
        }
        public Material BattleMaterial
        {
            get
            {
                if (!winnerIsDetermined && (battleMat is null || dirty))
                {
                    var curStage = curBattleSetDef.battleSetTextures[curGameStageInd];
                    battleMat = curStage.graphicData.Graphic.MatAt(Rotation);
                    dirty = false;
                    if (curStage.soundDef != null)
                    {
                        curStage.soundDef.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
                    }
                }
                else if (winningBattleSet != null && winnerIsDetermined && dirty)
                {
                    battleMat = winningBattleSet.graphicData.Graphic.MatAt(Rotation);
                    dirty = false;
                    if (winningBattleSet.soundDef != null)
                    {
                        winningBattleSet.soundDef.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
                    }
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

                Vector3 s = this.Rotation.IsHorizontal ? new Vector3(this.def.graphicData.drawSize.y, 1f, this.def.graphicData.drawSize.x) 
                        : new Vector3(this.def.graphicData.drawSize.x, 1f, this.def.graphicData.drawSize.y);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BattleMaterial, 0);
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (InUse && Find.TickManager.TicksGame >= nextTurnTick && !winnerIsDetermined)
            {
                if (curGameStageInd < curBattleSetDef.battleSetTextures.Count - 1)
                {
                    curGameStageInd++;
                    nextTurnTick = Find.TickManager.TicksGame + curBattleSetDef.battleSetTextures[curGameStageInd].ticksInterval.RandomInRange;
                    dirty = true;
                }
            }
        }
        public bool IsGameFinished => winnerIsDetermined && Find.TickManager.TicksGame >= nextTurnTick;
        public bool LastTurn => curGameStageInd == curBattleSetDef.battleSetTextures.Count - 1 && !winnerIsDetermined && Find.TickManager.TicksGame >= nextTurnTick;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Action()
            {
                defaultLabel = "RE.SelectBattleSet".Translate(this.curBattleSetDef.label),
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
                    this.curBattleSetDef = def;
                    this.curGameStageInd = 0;
                }, MenuOptionPriority.Default);
            }
        }

    }
}
