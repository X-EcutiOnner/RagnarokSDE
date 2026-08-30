using System;
using System.Collections.Generic;
using System.IO;
using GRF.Image;
using GRF.IO;
using GrfToWpfBridge;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Databases.ClientItems;
using SDE.Databases.ClientItems.Features;
using SDE.Editor.Database;
using SDE.Editor.Items;
using SDE.View;
using TokeiLibrary;
using Utilities.Extension;

namespace SDE.Editor.Validation {
	public class ResourceError : ValidationErrorView {
		public string MissingPath { get; set; }
		public GrfImageType ImageType { get; set; }

		protected DataSource _source;

		public ResourceError(ValidationErrors type, int itemId, string message, DataSource source, DbValidationEngine validationEngine, string path)
			: base(type, itemId, message, source, validationEngine) {
			MissingPath = path;
		}

		public override void GetCommands(HashSet<ValidationCommand> commands) {
			base.GetCommands(commands);

			var error = Error;
			Func<ValidationErrorView, bool> canExecute = t => t.Error == error;

			switch(Error) {
				case ValidationErrors.ResClientMissing:
					commands.Add(new ValidationCommand {
						CmdName = "fix_add_missing",
						Icon = "add.png",
						DisplayName = "Add missing client items",
						CanExecute = canExecute,
						_execute = t => {
							try {
								var sde = SdeEditor.Project;

								var citemDb = sde.GetDb(DataSources.ClientItem);
								var petDb1 = sde.GetDb(DataSources.Pet);
								var petDb2 = sde.GetDb(DataSources.PetImport);
								var mobDb1 = sde.GetDb(DataSources.Mob);
								var mobDb2 = sde.GetDb(DataSources.MobImport);
								var itemDb = sde.GetMergedTable(DataSources.Item);

								int id = t.Id;

								ReadableTuple tupleSource = citemDb.Table.TryGetTuple(id);

								if (tupleSource == null) {
									tupleSource = new ReadableTuple(id, ClientItemAttributes.AttributeList);
									citemDb.Table.Commands.AddTuple(id, tupleSource);
								}

								var cmds = new ItemGeneratorEngine<int>().Generate(tupleSource, itemDb.TryGetTuple(id), mobDb1, mobDb2, petDb1, petDb2);

								if (cmds != null) {
									citemDb.Table.Commands.StoreAndExecute(cmds);
								}
							}
							catch {
							}

							return true;
						}
					});
					break;
				case ValidationErrors.ResInvalidType:
					commands.Add(new ValidationCommand {
						CmdName = "fix_image_type",
						Icon = "convert.png",
						DisplayName = "Convert image type",
						CanExecute = canExecute,
						_execute = t => {
							var sde = SdeEditor.Project;

							GrfImage image = new GrfImage(sde.MetaGrf.GetData(((ResourceError)t).MissingPath));
							image.Convert(((ResourceError)t).ImageType);

							var path = GetNextPath();
							image.Save(path);

							t.ValidationEngine.Grf.Commands.AddFile(((ResourceError)t).MissingPath, path);
							return true;
						}
					});
					break;
				case ValidationErrors.ResEmpty:
					commands.Add(new ValidationCommand {
						CmdName = "fix_empty_resource",
						Icon = "add.png",
						DisplayName = "Add default resource name",
						CanExecute = canExecute,
						_execute = t => {
							var sde = SdeEditor.Project;

							var citemDb = sde.GetDb(DataSources.ClientItem);
							var citem = citemDb.Table.TryGetTuple(t.Id);

							if (citem != null) {
								var clientModel = citem.GetModel<ClientItem>();

								if (string.IsNullOrEmpty(clientModel.IdentifiedResourceName)) {
									citemDb.Table.Commands.SetModelValue(citem, clientModel, nameof(ClientItem.IdentifiedResourceName), "조각케이크".ToDisplayEncoding());
								}

								if (string.IsNullOrEmpty(clientModel.UnidentifiedResourceName)) {
									citemDb.Table.Commands.SetModelValue(citem, clientModel, nameof(ClientItem.UnidentifiedResourceName), "조각케이크".ToDisplayEncoding());
								}
							}

							return true;
						}
					});
					break;
				case ValidationErrors.ResInventory:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing inventory textures", "def_inv", canExecute));
					break;
				case ValidationErrors.ResCollection:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing collection textures", "def_col", canExecute));
					break;
				case ValidationErrors.ResIllustration:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing card illustration textures", "def_illust", canExecute));
					break;
				case ValidationErrors.ResDrag:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing drag sprites", "def_drag", canExecute));
					break;
				case ValidationErrors.ResHeadgear:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing headgear sprites", "def_head", canExecute));
					break;
				case ValidationErrors.ResShield:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing shield sprites", "def_head", canExecute));
					break;
				case ValidationErrors.ResGarment:
					commands.Add(_generateMissingCmd("spritemaker.png", "Add missing garment sprites", "def_head", canExecute));
					break;
			}
		}

		private ValidationCommand _generateMissingCmd(string icon, string displayName, string sdeResource, Func<ValidationErrorView, bool> canExecute) {
			return new ValidationCommand {
				CmdName = displayName,
				Icon = icon,
				DisplayName = displayName,
				CanExecute = canExecute,
				_execute = t => {
					var cpy = sdeResource;

					if (string.IsNullOrEmpty(cpy.GetExtension())) {
						cpy = cpy + ((ResourceError)t).MissingPath.GetExtension();
					}

					var path = GrfPath.Combine(SdeAppConfiguration.TempPath, cpy);

					if (!File.Exists(path)) {
						File.WriteAllBytes(path, ApplicationManager.GetResource(cpy));
					}

					ValidationEngine.Grf.Commands.AddFile(((ResourceError)t).MissingPath.ToDisplayEncoding(), path);
					return true;
				}
			};
		}
	}
}