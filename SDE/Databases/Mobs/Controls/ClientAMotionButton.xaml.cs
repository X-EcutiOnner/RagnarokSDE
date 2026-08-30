using GRF.FileFormats.ActFormat;
using GRF.IO;
using SDE.Databases.Generic.Controls;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.View;
using System;
using System.Windows;
using Utilities.Services;

namespace SDE.Databases.Mobs.Controls {
	public partial class ClientAMotionButton : MultiApplyBase {
		public ClientAMotionButton() {
			InitializeComponent();
		}

		protected override string _getNewValue(ReadableTuple tuple, object oModel, string srcValue, string oldValue, string newValue) {
			var model = (Mob)oModel;
			string clientSprite = model.ClientSprite;

			if (String.IsNullOrEmpty(clientSprite))
				return newValue;

			var actData = SdeEditor.MetaGrf.GetData(EncodingService.FromAnyToDisplayEncoding(GrfPath.Combine(@"data\sprite\¸ó½ºÅÍ\", clientSprite) + ".act"));

			if (actData == null)
				return newValue;

			Act act = new Act(actData);
			
			if (act.Actions.Count < 17 || act[16].Frames.Count < 2)
				return newValue;
			
			var interval = act[16].AnimationSpeed * 24f;
			
			var maxDamageDuration = (act[16].Frames.Count - 2) * interval;
			
			for (int fid = 0; fid < act[16].Frames.Count; fid++) {
				if (act[16, fid].GetSoundFileName(act) == "atk") {
					maxDamageDuration = fid * interval;
					break;
				}
			}

			return maxDamageDuration.ToString();
		}

		private void _button_Click(object sender, RoutedEventArgs e) {
			base.Execute();
		}
	}
}
