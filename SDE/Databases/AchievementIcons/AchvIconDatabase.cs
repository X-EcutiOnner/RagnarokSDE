using SDE.Editor.Database;
using SDE.View;
using System.IO;
using System.Linq;
using Utilities.Extension;
using Utilities.Services;

namespace SDE.Databases.AchievementIcons {
	public class AchvIconDatabase : BaseDatabase {
		public AchvIconDatabase() {
			Source = DataSources.AchievementIcon;
			AttributeList = AchvIconAttributes.AttributeList;
			TabGenerator = null;
			IsGenerateTab = false;
		}

		protected override void _loadDb() {
			var files = SdeEditor.MetaGrf.FilesInDirectory(EncodingService.FromAnsiToDisplayEncoding(@"data\texture\À¯ÀúÀÎÅÍÆäÀÌ½º\achievement_re\"));

			foreach (var file in files.Where(p => p.IsExtension(".bmp"))) {
				var path = Path.GetFileNameWithoutExtension(file);

				if (!path.StartsWith("icon_"))
					continue;

				int id = Table.GenerateUniqueId();
				string key = path.Substring("icon_".Length);
				var tuple = new ReadableTuple(id, AchvIconAttributes.AttributeList);
				tuple.SetRawValue(AchvIconAttributes.Value, path);
				tuple.SetRawValue(AchvIconAttributes.StringId, key.ToUpperInvariant());
				Table.Add(id, tuple);
			}
		}
	}
}
