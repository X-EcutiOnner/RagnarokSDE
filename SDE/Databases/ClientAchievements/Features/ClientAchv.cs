using SDE.Databases.ClientAchievements.Common;
using System;
using System.Collections.Generic;

namespace SDE.Databases.ClientAchievements.Features {
	public class ClientAchv : ICloneable {
		public string Title;

		// The Group is only used to retrieve the icon, it is otherwise meaningless.
		// The path goes: data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\achievement_re\icon_{Group}.bmp
		public string Group;
		public string Summary;
		public string Details;
		public string RewardItem;
		public string RewardTitle;
		public string RewardBuff;
		public string Score;
		public string Major;
		public string Minor;

		// This one is an enum...
		// 0: Display the completion list without a counter
		// 1: Display the completion list with a counter (either mob count or whatever count the server uses)
		public ClientAchvUiType UiType;
		public List<ClientAchvResource> Resources = new List<ClientAchvResource>();

		public object Clone() {
			var model = (ClientAchv)MemberwiseClone();

			model.Resources = new List<ClientAchvResource>();

			foreach (var resource in Resources)
				model.Resources.Add((ClientAchvResource)resource.Clone());

			return model;
		}
	}
}
