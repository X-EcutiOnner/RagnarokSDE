using SDE.Databases.Castles.Common;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Features;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Runtime.CompilerServices;

namespace SDE.Databases.Castles.Features {
	public class CastleViewModel : BaseModelView<Castle> {
		public bool IsLocked { get; set; }

		public CastleViewModel(DbTab tab) {
			Tab = tab;
		}

		public void SetModel(ReadableTuple tuple, Castle quest) {
			if (IsLocked)
				return;

			Model = quest;
			Tuple = tuple;

			OnPropertyChanged("");
		}

		public string Id {
			get => Tuple?.Key.ToString();
			set {
				if (Tuple.Key.ToString() == value)
					return;

				if (Int32.TryParse(value, out int key)) {
					Tab.Commands.ChangeKey(key);
				}

				OnPropertyChanged(nameof(Id));
			}
		}

		public string Map { get => Model?.Map; set => ExecuteCommand(value); }
		public string Name { get => Model?.Name; set => ExecuteCommand(value); }
		public string Npc { get => Model?.Npc; set => ExecuteCommand(value); }
		public EnumInfoBase Type { get => EnumInfos.GetEnumBase(Model?.Type); set => ExecuteCommand((CastleType)value.Value); }
		public string ClientId { get => Model?.ClientId; set => ExecuteCommand(value); }
		public bool WarpEnabled { get => Model == null ? false : Model.WarpEnabled; set => ExecuteCommand(value); }
		public string WarpX { get => Model?.WarpX; set => ExecuteCommand(value); }
		public string WarpY { get => Model?.WarpY; set => ExecuteCommand(value); }
		public string WarpCost { get => Model?.WarpCost; set => ExecuteCommand(value); }
		public string WarpCostSiege { get => Model?.WarpCostSiege; set => ExecuteCommand(value); }

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}
	}
}
