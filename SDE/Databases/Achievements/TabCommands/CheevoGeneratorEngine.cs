using System;
using System.Globalization;
using Database.Commands;
using SDE.Databases;
using SDE.Databases.Achievements.Features;
using SDE.Databases.ClientAchievements;
using SDE.Databases.ClientAchievements.Common;
using SDE.Databases.ClientAchievements.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Features;
using SDE.Editor.Database;
using SDE.View;

namespace SDE.Editor.Achievement {
	public class AchievementAutocomplete {
		public static GroupCommand<int, ReadableTuple> Autocomplete(int key, ReadableTuple serverTuple, bool createNew = false) {
			GroupCommand<int, ReadableTuple> commands = GroupCommand<int, ReadableTuple>.Make();
			var clientAchvDb = SdeEditor.Project.GetTable(DataSources.ClientAchievement);
			ReadableTuple clientTuple = clientAchvDb.TryGetTuple(key);

			// If autocomplete was requested for an existing client entry (createNew == false), this client entry must also exist.
			if (createNew == false && clientTuple == null)
				return null;

			// If autocomplete was requested to create a new entry (createNew == true), this client entry must not exist.
			if (createNew == true && clientTuple != null)
				return null;

			Achv server = serverTuple.GetModel<Achv>();
			ClientAchv client = null;

			if (clientTuple == null) {
				clientTuple = new ReadableTuple(key, ClientAchvAttributes.AttributeList);
				clientTuple.Added = true;
				clientAchvDb.Commands.AddTuple(key, clientTuple, false);

				client = new ClientAchv();
				clientAchvDb.Commands.Set(clientTuple, ClientAchvAttributes.Model, client);
			}
			else {
				client = clientTuple.GetModel<ClientAchv>();
			}

			if (createNew || ProjectConfiguration.AutocompleteRewardId) {
				int idC = DbReader.ToInt(client.RewardItem);
				int idS = DbReader.ToInt(server.RewardItem);

				if (idC != idS) {
					commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client, nameof(ClientAchv.RewardItem), idS.ToString()));
				}
			}

			if (createNew || ProjectConfiguration.AutocompleteTitleId) {
				int idC = DbReader.ToInt(client.RewardTitle);
				int idS = DbReader.ToInt(server.RewardTitleId);

				if (idC != idS) {
					commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client, nameof(ClientAchv.RewardTitle), idS.ToString()));
				}
			}

			if (createNew || ProjectConfiguration.AutocompleteScore) {
				int idC = DbReader.ToInt(client.Score);
				int idS = DbReader.ToInt(server.Score);

				if (idC != idS) {
					commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client, nameof(ClientAchv.Score), idS.ToString()));
				}
			}

			if (createNew || ProjectConfiguration.AutocompleteName) {
				string idC = client.Title ?? "";
				string idS = server.Name ?? "";

				if (idC != idS) {
					commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client, nameof(ClientAchv.Title), idS));
				}
			}

			if (createNew) {
				string idC = client.Group ?? "";
				string idS = server.Group.ToString();

				if (String.Compare(idC, idS, StringComparison.OrdinalIgnoreCase) != 0) {
					commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client, nameof(ClientAchv.Group), idS.Replace("AG_", "").ToUpperInvariant()));
				}
			}

			// No need for commands, can be applied directly
			if (createNew) {
				string name = server.Name;

				client.Summary = name;
				client.Details = name;
				client.Major = "0";
				client.Minor = "0";

				if (server.Targets.Count > 0) {
					client.UiType = ClientAchvUiType.UITYPE_TEXT_AND_COUNTER;
					var mobDb = SdeEditor.Project.GetMergedTable(DataSources.Mob);

					for (int i = 0; i < server.Targets.Count; i++) {
						var target = server.Targets[i];

						Int32.TryParse(target.Count, out int count);
						Int32.TryParse(target.Mob, out int mobId);

						if (count > 0) {
							string text = "Task " + (i + 1);
							
							if (mobId > 0) {
								var tuple = mobDb.TryGetTuple(mobId);
							
								if (tuple != null) {
									string mobName = tuple.GetModel<Mob>().Name;
							
									text = "Defeat " + mobName + " " + (count == 1 ? "once!" : count + " times!");
								}
							}

							client.Resources.Add(new ClientAchvResource() { Id = (i + 1).ToString(), Text = text, Count = count.ToString() });
						}
					}
				}
			}

			if (!createNew && ProjectConfiguration.AutocompleteCount) {
				var uiType = client.UiType;
				var group = server.Group;

				if (uiType == ClientAchvUiType.UITYPE_TEXT_AND_COUNTER && group != Databases.Achievements.Common.AchvGroupType.AG_SPEND_ZENY) {
					for (int i = 0; i < server.Targets.Count; i++) {
						var target = server.Targets[i];
						Int32.TryParse(target.Count, out int targetCount);

						if (i < client.Resources.Count && targetCount != DbReader.ToInt(client.Resources[i].Count)) {
							if (client.Resources[i].Text.Contains(client.Resources[i].Count ?? "")) {
								var newText = client.Resources[i].Text.Replace(client.Resources[i].Count ?? "", targetCount.ToString(CultureInfo.InvariantCulture));
								commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client.Resources[i], nameof(ClientAchvResource.Text), newText));
							}

							commands.Add(new ModelCommand<int, ReadableTuple, string>(clientTuple, client.Resources[i], nameof(ClientAchvResource.Count), targetCount.ToString()));
						}
					}
				}
			}

			if (commands.Commands.Count == 0)
				return null;

			return commands;
		}
	}
}