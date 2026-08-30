using SDE.Editor.Generic.DbTabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDE.Databases.Generic.Features {
	public interface IDatabaseView {
		void Init(DbTab tab);
	}
}
