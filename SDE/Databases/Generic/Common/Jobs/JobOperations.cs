using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.Databases.Generic.Common.Jobs {
	public static class JobGroups {
		public static UInt64 EverySdeJobs;
		public static UInt64 EveryFirstJobs;
		public static UInt64 EverySecondJobs;
		public static UInt64 EveryTransJobs;
		public static UInt64 EveryBabyJobs;
		public static UInt64 EveryFourthJobs;
		public static UInt64 EveryThirdJobs;
		public static UInt64 EveryJob;

		static JobGroups() {
			EveryFirstJobs = Job.GetGroupId(Job.Swordman, Job.Mage, Job.Archer, Job.Acolyte, Job.Merchant, Job.Thief);
			EverySecondJobs = Job.GetGroupId(Job.Knight, Job.Wizard, Job.Hunter, Job.Priest, Job.Blacksmith, Job.Assassin, Job.Crusader, Job.Sage, Job.BardDancer, Job.Monk, Job.Alchemist, Job.Rogue);

			foreach (var job in Job.AllJobs) {
				if ((job.MapId & (MAPIDs)JOBLs­.JOBL_UPPER) != 0)
					EveryTransJobs |= job.JobSdeUid;
				if ((job.MapId & (MAPIDs)JOBLs­.JOBL_BABY) != 0)
					EveryBabyJobs |= job.JobSdeUid;
				if ((job.MapId & (MAPIDs)JOBLs­.JOBL_FOURTH) != 0)
					EveryFourthJobs |= job.JobSdeUid;
				if ((job.MapId & (MAPIDs)JOBLs­.JOBL_THIRD) != 0)
					EveryThirdJobs |= job.JobSdeUid;

				EveryJob |= job.JobSdeUid;
				//EveryFirstJobs |= job.BaseJob.JobSdeUid;
			}

			foreach (var job in Job.PrimaryJobs) {
				EverySdeJobs |= job.JobSdeUid;
			}
		}
	}

	public static class JobSdeIdGroups {
		public static UInt64 AllJobs;
		public static UInt64 AllPrimaryJob;
		public static UInt64 All;
		public static UInt64 Renewal;
		public static UInt64 ThirdAbove;

		static JobSdeIdGroups() {
			AllJobs = (1UL << Job.MaxJobUid) - 1;

			foreach (var job in Job.PrimaryJobs)
				AllPrimaryJob |= job.JobSdeUid;
		}
	}

	public static class JobOperations {
		public static JOBLs ItemJobFlag2JOBL(ItemJobFlag flag) {
			JOBLs r = 0;
			
			if ((flag & ItemJobFlag.ITEMJ_UPPER) == ItemJobFlag.ITEMJ_UPPER)
				r |= JOBLs.JOBL_UPPER;
			if ((flag & ItemJobFlag.ITEMJ_BABY) == ItemJobFlag.ITEMJ_BABY)
				r |= JOBLs.JOBL_BABY;
			if ((flag & ItemJobFlag.ITEMJ_THIRD) == ItemJobFlag.ITEMJ_THIRD)
				r |= JOBLs.JOBL_THIRD;
			if ((flag & ItemJobFlag.ITEMJ_THIRD_UPPER) == ItemJobFlag.ITEMJ_THIRD_UPPER)
				r |= JOBLs.JOBL_THIRD | JOBLs.JOBL_UPPER;
			if ((flag & ItemJobFlag.ITEMJ_THIRD_BABY) == ItemJobFlag.ITEMJ_THIRD_BABY)
				r |= JOBLs.JOBL_THIRD | JOBLs.JOBL_BABY;
			if ((flag & ItemJobFlag.ITEMJ_FOURTH) == ItemJobFlag.ITEMJ_FOURTH)
				r |= JOBLs.JOBL_FOURTH;

			return r;
		}

		//public static List<Job> GetJobs(UInt64 jobFlag) {
		//	return Job.AllJobs.Where(p => p.Restrict(classBase)).ToList();
		//}

		public static List<Job> GetJobs(UInt64 jobSdeFlag) {
			return Job.PrimaryJobs.Where(job => (job.JobSdeUid & jobSdeFlag) != 0).ToList();
		}

		public static List<Job> GetJobs(UInt64 jobSdeFlag, ItemJobFlag upper) {
			JOBLs classGroup = ItemJobFlag2JOBL(upper);
			
			List<Job> jobs = new List<Job>();

			foreach (var job in Job.PrimaryJobs) {
				if ((job.JobSdeUid & jobSdeFlag) == 0)
					continue;

				jobs.Add(Job.Get(job, upper));
			}

			return jobs;
		}

		public static List<Job> GetAllJobs(UInt64 jobSdeFlag, ItemJobFlag upper) {
			return Job.AllJobs.Where(p => (p.JobSdeUid & jobSdeFlag) != 0 && p.CanUseItem(upper)).ToList();
		}

		public static string GetStringFormat(UInt64 jobSdeFlag, ItemJobFlag upper, GenderType gender, int equipLevel) {
			var genderString = GenderString(gender);
			var output1 = genderString + GenerateGroupJobs(jobSdeFlag & JobGroups.EverySdeJobs, upper);
			var output2 = GetStringFormatSub(jobSdeFlag, upper, gender, equipLevel);

			if (output1.Length < output2.Length)
				return output1;

			return output2;
		}

		public static string GetStringFormatSub(UInt64 jobSdeFlag, ItemJobFlag upper, GenderType gender, int equipLevel) {
			var genderString = GenderString(gender);

			// What these checks mean:
			// The parameter that has the highest priority is the ItemJobFlag value.
			// As an example, if ItemJobFlag is ITEMJ_THIRD_UPPER, then only 3rd trans classes can be used.
			// However, for that to be true, all selected jobs (from jobSdeFlag) must also be active.
			// It's only checking the subset because even if you had a non-trans jobs listed, it wouldn't matter since
			// the ItemJobFlag would make this job not available anyway. Hence why we only need to check if 
			// jobSdeFlag is a subset of EveryTransJobs.
			//
			// Also, those strings aren't always precise. Take "Every Trans 3rd Job". While true, that also implies
			// that any fourth classes can use this item. It's just something players have adapted to due to outdated item translations.
			// Though, making them clearer might not be a bad idea... something such as "Every Trans 3rd/4th Job".
			// That also relies on whether 4th jobs are enabled or not, but that's a worry for another time.

			//// There aren't enough jobs to justify writing exceptions, writing them plainly is cleaner.
			//if (jobs.Count <= 6) {
			//	return genderString + GenerateGroupJobs(jobSdeFlag & JobGroups.EverySdeJobs, upper);
			//}

			// Trans jobs
			if (upper == ItemJobFlag.ITEMJ_UPPER)
				return genderString + "Every Trans 1st or 2nd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.ITEMJ_UPPER);
			if (upper == ItemJobFlag.ITEMJ_THIRD_UPPER)
				return genderString + "Every Trans 3rd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.ITEMJ_THIRD_UPPER);
			if (upper == ItemJobFlag.Trans)
				return genderString + "Every Trans Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.Trans);
			if (upper == ItemJobFlag.ITEMJ_ALL_UPPER)
				return genderString + "Every Trans or 4th Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.ITEMJ_ALL_UPPER);
			if (upper == ItemJobFlag.TransAndThird)
				return genderString + "Every Trans Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.TransAndThird);
			if (upper == (ItemJobFlag.ThirdAbove | ItemJobFlag.ITEMJ_UPPER)) {
				// rAthena messed up a bit with this one. They made all "trans only" flags into third class + trans. It's not wrong, but it makes printing names a bit harder.
				if ((jobSdeFlag & ~JobGroups.EveryTransJobs) == 0)
					return genderString + "Every Trans Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryTransJobs, ItemJobFlag.TransAndThird);

				return genderString + "Every Trans, 3rd or 4th Job" + GenerateExceptGroupJobs(~jobSdeFlag & (JobGroups.EveryTransJobs | JobGroups.EveryThirdJobs), ItemJobFlag.ThirdAbove | ItemJobFlag.ITEMJ_UPPER);
			}

			// Baby jobs
			if (upper == ItemJobFlag.ITEMJ_BABY)
				return genderString + "Every Baby 1st or 2nd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryBabyJobs, ItemJobFlag.ITEMJ_BABY);
			if (upper == ItemJobFlag.ITEMJ_THIRD_BABY)
				return genderString + "Every Baby 3rd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryBabyJobs, ItemJobFlag.ITEMJ_THIRD_BABY);
			if (upper == ItemJobFlag.ITEMJ_ALL_BABY)
				return genderString + "Every Baby Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryBabyJobs, ItemJobFlag.ITEMJ_ALL_BABY);

			// Third jobs and fourth jobs
			if (upper == ItemJobFlag.ITEMJ_THIRD)
				return genderString + "Every 3rd Job (excluding Trans and Baby)" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryThirdJobs, ItemJobFlag.ITEMJ_THIRD);
			if (upper == ItemJobFlag.ITEMJ_ALL_THIRD)
				return genderString + "Every 3rd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryThirdJobs, ItemJobFlag.ITEMJ_ALL_THIRD);
			if (upper == ItemJobFlag.PreRenewal)
				return genderString + "Every 1st or 2nd Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EverySdeJobs, ItemJobFlag.PreRenewal);
			if (upper == ItemJobFlag.ITEMJ_FOURTH)
				return genderString + "Every 4th Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryFourthJobs, ItemJobFlag.ITEMJ_FOURTH);
			if (upper == ItemJobFlag.ThirdAbove)
				return genderString + "Every 3rd or 4th Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EveryThirdJobs, ItemJobFlag.ThirdAbove);

			// No more groups found, at this point, we just enumerate all jobs one by one instead.
			// But there's a slight trick to simplify this process. We can use instead just list the missing jobs,
			// as it's often faster. If the available jobs are less than 50%, then use the "except" format instead.
			// ^ scrap that, check if the flag is negative instead.
			var jobs = GetJobs(jobSdeFlag).Where(p => Job.Get(p, upper).CanUseItem(upper)).ToList();

			//if (jobs.Count > 0.5 * Job.PrimaryJobs.Count) {
			if ((long)jobSdeFlag < 0) {
				return genderString + "Every Job" + GenerateExceptGroupJobs(~jobSdeFlag & JobGroups.EverySdeJobs, upper);
			}
			else {
				return genderString + GenerateGroupJobs(jobSdeFlag & JobGroups.EverySdeJobs, upper);
			}
		}

		public class JobGroupLink {
			public Job Parent;
			public Job Child2_1;
			public Job Child2_2;

			public ulong ToSdeId() {
				return Parent.JobSdeUid | (Child2_1 != null ? Child2_1.JobSdeUid : 0) | (Child2_2 != null ? Child2_2.JobSdeUid : 0);
			}
		}

		public static List<JobGroupLink> JobLinks = new List<JobGroupLink>();

		public static void Init() {
			foreach (var job in Job.FirstJobs) {
				var job2_1 = Job.TryGet(job.MapId | (MAPIDs)JOBLs.JOBL_2_1);
				var job2_2 = Job.TryGet(job.MapId | (MAPIDs)JOBLs.JOBL_2_2);

				JobLinks.Add(new JobGroupLink() { Parent = job, Child2_1 = job2_1, Child2_2 = job2_2 });
			}
		}

		public static string GenerateGroupJobs(UInt64 jobSdeId, ItemJobFlag upper) {
			if (JobLinks.Count == 0)
				Init();

			string output = "";
			string prefix = "";

			if ((upper & ItemJobFlag.AllBelowThird) == 0)
				prefix = "3rd ";
			if (upper == ItemJobFlag.ITEMJ_FOURTH)
				prefix = "4th ";

			// First pass, add groups and detect for the "3rd" prefix
			foreach (var jobLink in JobLinks) {
				// Check if parent is possible
				var family = jobLink.ToSdeId();

				if (!Job.Get(jobLink.Parent, upper).CanUseItem(upper)) {
					jobSdeId &= ~jobLink.Parent.JobSdeUid;
					family &= ~jobLink.Parent.JobSdeUid;
				}

				if (family != 0 && (jobSdeId & family) == family) {
					output += Job.Get(jobLink.Parent, upper) + " Class, ";
					jobSdeId &= ~family;
				}
			}

			if (output != "")
				output = prefix + output;

			// Second pass, add remainders
			foreach (var jobLink in JobLinks) {
				if ((jobSdeId & jobLink.Parent.JobSdeUid) != 0) {
					output += Job.Get(jobLink.Parent, upper, 0) + ", ";
					jobSdeId &= ~jobLink.Parent.JobSdeUid;
				}

				if (jobLink.Child2_1 != null && (jobSdeId & jobLink.Child2_1.JobSdeUid) != 0) {
					output += Job.Get(jobLink.Child2_1, upper, 0) + ", ";
					jobSdeId &= ~jobLink.Child2_1.JobSdeUid;
				}

				if (jobLink.Child2_2 != null && (jobSdeId & jobLink.Child2_2.JobSdeUid) != 0) {
					output += Job.Get(jobLink.Child2_2, upper, 0) + ", ";
					jobSdeId &= ~jobLink.Child2_2.JobSdeUid;
				}
			}

			return output.TrimEnd(',', ' ');
		}

		public static string GenerateExceptGroupJobs(UInt64 jobSdeId, ItemJobFlag upper) {
			var r = GenerateGroupJobs(jobSdeId, upper);

			if (r != "")
				r = " except " + r;

			return r;
		}

		private static Dictionary<int, ulong> _csv2JobSdeFlag = new Dictionary<int, ulong> {
			{ 1 << 0, Job.Novice.JobSdeUid },
			{ 1 << 1, Job.Swordman.JobSdeUid },
			{ 1 << 2, Job.Mage.JobSdeUid },
			{ 1 << 3, Job.Archer.JobSdeUid },
			{ 1 << 4, Job.Acolyte.JobSdeUid },
			{ 1 << 5, Job.Merchant.JobSdeUid },
			{ 1 << 6, Job.Thief.JobSdeUid },
			{ 1 << 7, Job.Knight.JobSdeUid },
			{ 1 << 8, Job.Priest.JobSdeUid },
			{ 1 << 9, Job.Wizard.JobSdeUid },
			{ 1 << 10, Job.Blacksmith.JobSdeUid },
			{ 1 << 11, Job.Hunter.JobSdeUid },
			{ 1 << 12, Job.Assassin.JobSdeUid },
			//{ 1 << 13, Job.?.JobSdeUid },
			{ 1 << 14, Job.Crusader.JobSdeUid },
			{ 1 << 15, Job.Monk.JobSdeUid },
			{ 1 << 16, Job.Sage.JobSdeUid },
			{ 1 << 17, Job.Rogue.JobSdeUid },
			{ 1 << 18, Job.Alchemist.JobSdeUid },
			{ 1 << 19, Job.BardDancer.JobSdeUid },
			//{ 1 << 20, Job.?.JobSdeUid },
			{ 1 << 21, Job.Taekwon.JobSdeUid },
			{ 1 << 22, Job.StarGladiator.JobSdeUid },
			{ 1 << 23, Job.SoulLinker.JobSdeUid },
			{ 1 << 24, Job.Gunslinger.JobSdeUid },
			{ 1 << 25, Job.Ninja.JobSdeUid },
			//{ 1 << 26, Job.?.JobSdeUid },
			//{ 1 << 27, Job.?.JobSdeUid },
			//{ 1 << 28, Job.?.JobSdeUid },
			{ 1 << 29, Job.KagerouOboro.JobSdeUid },
			{ 1 << 30, Job.Rebellion.JobSdeUid },
			{ 1 << 31, Job.Summoner.JobSdeUid },
		};

		private static Dictionary<ulong, int> _jobSdeFlag2Csv = new Dictionary<ulong, int>();

		static JobOperations() {
			foreach (var entry in _csv2JobSdeFlag)
				_jobSdeFlag2Csv[entry.Value] = entry.Key;
		}

		public static UInt64 CsvFlag2SdeFlag(int jobAthenaId) {
			if (jobAthenaId == -1) {
				long r = -1;
				return (UInt64)r;
			}

			UInt64 jobSdeId = 0;

			foreach (var entry in _csv2JobSdeFlag) {
				if ((jobAthenaId & entry.Key) != 0)
					jobSdeId |= entry.Value;
			}

			return jobSdeId;
		}

		public static int SdeFlag2CsvFlag(UInt64 jobSdeId) {
			int jobAthenaId = 0;

			if ((long)jobSdeId == -1) {
				return -1;
			}

			foreach (var entry in _jobSdeFlag2Csv) {
				if ((jobSdeId & entry.Key) != 0)
					jobAthenaId |= entry.Value;
			}

			return jobAthenaId;
		}

		public static string GenderString(GenderType gender) {
			if (gender == GenderType.SEX_FEMALE)
				return "Female Only, ";
			if (gender == GenderType.SEX_MALE)
				return "Male Only, ";
			return "";
		}
	}
}
