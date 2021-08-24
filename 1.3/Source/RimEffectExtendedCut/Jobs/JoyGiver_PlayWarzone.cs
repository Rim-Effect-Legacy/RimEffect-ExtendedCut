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
	public class JoyGiver_PlayWarzone : JoyGiver_InteractBuilding
	{
		protected override bool CanDoDuringGathering => true;
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			var warzoneTable = t as Building_WarzoneTable;
			if (warzoneTable is null || !warzoneTable.CanUse || warzoneTable.InUse || !pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				return null;
			}
			var companion = FindCompanion(pawn);
			if (companion != null)
            {
				return JobMaker.MakeJob(def.jobDef, warzoneTable, companion);
			}
			return null;
		}

		protected Pawn FindCompanion(Pawn initiator)
		{
			var candidates = initiator.Map.mapPawns.SpawnedPawnsInFaction(initiator.Faction).Where(candidate => candidate != initiator && BasePawnValidator(candidate)
				&& MemberValidator(candidate) && PawnsCanGatherTogether(initiator, candidate));
			if (candidates.Any() && candidates.TryRandomElementByWeight(x => SortCandidatesBy(initiator, x), out var companion))
			{
				return companion;
			}
			return null;
		}

		protected bool MemberValidator(Pawn pawn)
		{
			var value = !workTags.Contains(pawn.mindState.lastJobTag);
			return value;
		}
		protected bool PawnsCanGatherTogether(Pawn organizer, Pawn companion)
		{
			return companion.relations.OpinionOf(organizer) >= 0 && organizer.relations.OpinionOf(companion) >= 0;
		}
		protected float SortCandidatesBy(Pawn organizer, Pawn candidate)
		{
			return organizer.relations.OpinionOf(candidate);
		}

		private bool BasePawnValidator(Pawn pawn)
		{
			var value = pawn.RaceProps.Humanlike && !pawn.InBed() && !pawn.InMentalState && pawn.GetLord() == null
			&& (pawn.timetable is null || pawn.timetable.CurrentAssignment.allowJoy) && !pawn.Drafted;
			return value;
		}

		private static HashSet<JobTag> workTags = new HashSet<JobTag> { JobTag.Misc, JobTag.MiscWork, JobTag.Fieldwork };

	}
}
