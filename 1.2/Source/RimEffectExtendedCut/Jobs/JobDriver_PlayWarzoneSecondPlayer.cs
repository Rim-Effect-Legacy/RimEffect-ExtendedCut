using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimEffectExtendedCut
{
    public class JobDriver_PlayWarzoneSecondPlayer : JobDriver
    {
        private Building_WarzoneTable Building_Warzone => this.TargetA.Thing as Building_WarzoneTable;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(Building_Warzone.GetSecondSpot(), PathEndMode.OnCell);
            Toil doPlay = new Toil();
            doPlay.tickAction = delegate
            {
                if (Building_Warzone.IsGameFinished)
                {
                    ReadyForNextToil();
                }
                else
                {
                    JoyUtility.JoyTickCheckEnd(pawn);
                }
            };
            doPlay.defaultCompleteMode = ToilCompleteMode.Never;
            doPlay.activeSkill = (() => SkillDefOf.Intellectual);
            doPlay.socialMode = RandomSocialMode.Normal;
            yield return doPlay;
        }
    }
}