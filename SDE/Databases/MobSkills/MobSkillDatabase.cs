using Database;
using ErrorManager;
using SDE.Databases.Generic.TabCommands;
using SDE.Databases.MobSkills.Common;
using SDE.Databases.MobSkills.Features;
using SDE.Databases.MobSkills.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Windows;
using TokeiLibrary.WPF;

namespace SDE.Databases.MobSkills {
	public class MobSkillDatabase : ModelDatabase {
		public MobSkillDatabase() : base(MobSkillAttributes.Model) {
			Source = DataSources.MobSkill;
			AttributeList = MobSkillAttributes.AttributeList;
			Parser = new MobSkillParserProvider();
			TabGenerator.OnInitSettings += delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.CustomAddItemMethod = delegate {
					try {
						int uid = Table.GenerateUniqueId();

						ReadableTuple item = new ReadableTuple(uid, settings.AttributeList);
						item.Added = true;

						Table.Commands.AddTuple(uid, item, false);
						tab.ListView.ScrollToCenterOfView(item);
					}
					catch (KeyInvalidException) {
					}
					catch (Exception err) {
						ErrorHandler.HandleException(err);
					}
				};
			};
			TabGenerator.OnInitSettings += delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.AttributeList = db.AttributeList;
				settings.AttId = MobSkillAttributes.DisplayMobId;
				settings.AttDisplay = MobSkillAttributes.Display;
				settings.AttIdWidth = 60;
			};
			TabGenerator.OnSetCustomCommands = delegate (DbTab tab, TabSettings settings, BaseDatabase db) {
				settings.HasUniqueId = true;
				settings.RemoveCommand(TabCommandAnchors.ChangeId);
				settings.AddCommand(TabCommandAnchors.CopyTo,
					new CopyToImportTable(this),
					new CopyToClipboard(this),
					new MobSkillSelectFromTable(DataSources.Skill),
					new MobSkillSelectFromTable(DataSources.MobSkill)
				);
			};
			UseUniqueId = true;

			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableLoaded), OnTableModified);
			WeakEventManager<BaseDatabase, EventArgs>.AddHandler(this, nameof(TableModified), OnTableModified);
		}

		public override FrameworkElement OnCreateTab(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Txt:
					return new MobSkillViewCsv();
				default:
					throw new Exception($"Unknown table format for '{Source}'. File type received: {format}.");
			}
		}

		public override void OnSetupSearchDescriptor(FileType format, DbTab tab, TabSettings settings, BaseDatabase db) {
			switch (format) {
				case FileType.Txt:
					SearchDescriptor = new Generic.SearchDescriptors.SearchDescriptor();
					SearchDescriptor.Add(v => ((MobSkill)v).MobId, "Mob ID", true);
					SearchDescriptor.Add(v => ((MobSkill)v).FriendlyDisplay, "Display name", true);
					SearchDescriptor.Add<MobSkillStateType>(v => ((MobSkill)v).State, "State", true);
					SearchDescriptor.Add<MobSkillTargetType>(v => ((MobSkill)v).Target, "Target", true);
					tab.SetupSearch(TabGenerator, settings, db);
					break;
			}
		}

		public void OnTableModified(object sender, EventArgs args) {
			DbUtilities.MobSkillDirty = true;
		}
	}

	public class MobSkillDatabaseImport : MobSkillDatabase {
		public MobSkillDatabaseImport() {
			Source = DataSources.MobSkillImport;
			ThrowFileNotFoundException = false;
		}
	}
}
