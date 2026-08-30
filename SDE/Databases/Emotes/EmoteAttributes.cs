using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDE.Databases.Emotes {
	public sealed class EmoteAttributes : DbAttribute {
		public static readonly AttributeList AttributeList = new AttributeList();

		public static readonly DbAttribute Id = new EmoteAttributes(new PrimaryAttribute("Id", typeof(int), "", "Emote ID"));
		public static readonly DbAttribute Emote = new EmoteAttributes(new DbAttribute("Emote", typeof(string), ""));

		private EmoteAttributes(DbAttribute attribute)
			: base(attribute) {
			AttributeList.Add(this);
		}
	}
}
