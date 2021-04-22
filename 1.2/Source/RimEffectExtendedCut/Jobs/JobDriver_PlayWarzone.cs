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
	public class JobDriver_PlayWarzone : JobDriver
	{
		private Building_WarzoneTable Building_Warzone => this.TargetA.Thing as Building_WarzoneTable;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}
        public override void Notify_Starting()
        {
            base.Notify_Starting();
			this.TargetB.Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RE_DefOf.RE_Play_WarzoneSecondPlayer, Building_Warzone, pawn));
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(Building_Warzone.GetFirstSpot(), PathEndMode.OnCell);
			Toil doPlay = new Toil();
			doPlay.tickAction = delegate
			{
				var table = Building_Warzone;
				if (!table.InUse && TargetB.Pawn.Position == table.GetSecondSpot())
                {
					table.StartPlay(this.pawn);
				}
				if (table.IsGameFinished)
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
			doPlay.AddFinishAction(() => Building_Warzone.StopPlay());
			yield return doPlay;
		}
	}
}
