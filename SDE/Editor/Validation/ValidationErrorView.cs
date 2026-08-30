using System.Collections.Generic;
using System.Linq;
using GRF.GrfSystem;
using SDE.Databases;
using SDE.Editor.Navigation;
using TokeiLibrary;

namespace SDE.Editor.Validation {
	public class ValidationErrorView {
		public static string GetNextPath() {
			return TemporaryFilesManager.GetTemporaryFilePath("va_{0:000}.bmp");
		}

		public ValidationErrorView(ValidationErrors type, int itemId, string message, DataSource source, DbValidationEngine validationEngine) {
			Source = source;
			Error = type;
			Message = message;
			Id = itemId;
			ValidationEngine = validationEngine;
		}

		public ValidationErrors Error { get; set; }
		public string ErrorString => Error.ToString();
		public string Message { get; set; }
		public int Id { get; set; }
		public DataSource Source { get; set; }
		public bool Default => true;
		public DbValidationEngine ValidationEngine { get; set; }

		public object DataImage {
			get {
				switch (Error) {
					case ValidationErrors.Generic:
					case ValidationErrors.ResInvalidName:
					case ValidationErrors.ResInvalidType:
					case ValidationErrors.ResClientMissing:
						return ApplicationManager.PreloadResourceImage("error16.png");
					case ValidationErrors.ResIllustration:
						return ApplicationManager.PreloadResourceImage("card.png");
					case ValidationErrors.ResInventory:
					case ValidationErrors.ResCollection:
						return ApplicationManager.PreloadResourceImage("spritemaker.png");
					case ValidationErrors.CiParseError:
					case ValidationErrors.TbCapValue:
					case ValidationErrors.TbGender:
						return ApplicationManager.PreloadResourceImage("help.png");
					case ValidationErrors.ResDrag:
						return ApplicationManager.PreloadResourceImage(Message.EndsWith(".spr") ? "file_spr.png" : "file_act.png");
					default:
						return ApplicationManager.PreloadResourceImage("warning16.png");
				}
			}
		}

		public override string ToString() {
			return string.Join("\t", new string[] { ErrorString, Id.ToString(), Message });
		}

		public virtual void GetCommands(HashSet<ValidationCommand> commands) {
			commands.Add(new ValidationCommand {
				CmdName = "select",
				Icon = "arrowdown.png",
				DisplayName = "Select in GRF",
				GroupCommand = true,
				_executeGroup = (t, l) => {
					TabNavigation.SelectList(t.Source, l.Select(p => p.Id));
					return false;
				},
				_execute = t => {
					TabNavigation.Select(t.Source, t.Id);
					return false;
				}
			});
		}
	}
}