using System;

namespace SDE.Databases.Generic.Common {
	[Flags]
	public enum JOBLs : UInt64 {
		//These marks the "level" of the job.
		JOBL_2_1 = 0x100,
		JOBL_2_2 = 0x200,
		JOBL_2 = 0x300,
		JOBL_THIRD = 0x1000,
		JOBL_FOURTH = 0x10000,

		//These marks the "version" of the job.
		JOBL_UPPER = 0x100000,
		JOBL_BABY = 0x200000,
	}
}
