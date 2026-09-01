using SDE.Core;
using SDE.Databases.Emotes;
using SDE.Editor.Database;
using System;
using System.Linq;

namespace SDE.Databases.Pets {
	public class EmoteDatabase : BaseDatabase {
		public EmoteDatabase() {
			Source = DataSources.Emote;
			AttributeList = EmoteAttributes.AttributeList;
			TabGenerator = null;
			IsGenerateTab = false;
		}

		protected override void _loadDb() {
			var emotionString = ResourceString.Get("Emotions");
			var emotions = emotionString.Split('\n').ToList();

			for (int i = 0; i < emotions.Count; i++) {
				var emotion = emotions[i].Trim('\r');
				string[] values = emotion.Split('\t');

				if (values.Length != 3)
					continue;

				int key = Int32.Parse(values[2]);
				var tuple = new ReadableTuple(key, EmoteAttributes.AttributeList);
				tuple.SetRawValue(EmoteAttributes.Emote, values[0] + ", " + values[1]);
				Table.Add(key, tuple);
			}
		}
	}
}
