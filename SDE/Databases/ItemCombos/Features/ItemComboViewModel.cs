using Database.Commands;
using SDE.Databases.Generic.Features;
using SDE.Databases.ItemCombos.Parser;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using TokeiLibrary.WPF;
using Utilities;

namespace SDE.Databases.ItemCombos.Features {
	public class ItemComboViewModel : BaseModelView<ItemCombo> {
		public RangeObservableCollection<NameIdViewModel> NameIds { get; } = new RangeObservableCollection<NameIdViewModel>();
		public RangeObservableCollection<ItemComboViewModel> LinkedCombos { get; } = new RangeObservableCollection<ItemComboViewModel>();
		
		public bool IsLocked { get; set; }
		public bool IsCurrent {
			get => Tuple.GetModel<ItemCombo>() == Model;
		}

		public ItemComboViewModel(DbTab tab) {
			Tab = tab;

			ApplyScriptCommand = new RelayCommand(OnApplyScriptCommand);
		}

		public ItemComboViewModel(ReadableTuple tuple, ItemCombo sourceModel) {
			Model = sourceModel;
			Tuple = tuple;

			OnNameIdsListUpdated();
			OnPropertyChanged("");
		}

		public void SetModel(ReadableTuple tuple, ItemCombo model) {
			if (IsLocked)
				return;

			var oldModel = Model;
			Model = model;
			Tuple = tuple;

			OnNameIdsListUpdated();

			if (oldModel != Model)
				OnLinkedCombosListUpdated();

			OnPropertyChanged("");
		}


		public Brush ForegroundBrush {
			get {
				if (IsCurrent)
					return ReadableTupleBrush.CellBrushEncrypted;

				return ReadableTupleBrush.TextForeground;
			}
		}

		public ICommand ApplyScriptCommand { get; set; }

		public string DisplayComboId {
			get => Tuple.GetValue<string>(ItemComboAttributes.DisplayId);
		}

		public string DisplayComboName {
			get => Tuple.GetValue<string>(ItemComboAttributes.DisplayName2);
		}

		public string Script {
			get => Model?.Script;
			set => ExecuteCommand(value);
		}

		public void ExecuteCommand<T>(T value, [CallerMemberName] string fieldName = "") {
			Execute(Model, value, fieldName, v => IsLocked = v);
		}

		public void OnApplyScriptCommand() {
			try {
				IsLocked = true;
				var tab = Tab;

				List<ReadableTuple> tuples = LinkedCombos.Select(p => p.Tuple).ToList();

				tab.Table.Commands.SetModelsValue<ItemCombo, string>(tuples, nameof(Script), Script);
			}
			finally {
				IsLocked = false;
			}

			OnPropertyChanged(nameof(Script));
		}

		public void OnNameIdsListUpdated() {
			List<NameIdViewModel> viewModels = new List<NameIdViewModel>();

			for (int i = 0; i < ItemCombo.MaxNameIdCount; i++) {
				if (Model == null || i >= Model.NameIds.Count) {
					viewModels.Add(new NameIdViewModel(this, new NameId()));
				}
				else {
					viewModels.Add(new NameIdViewModel(this, Model.NameIds[i]));
				}
			}

			NameIds.ClearAndAddRange(viewModels);
		}

		public void OnLinkedCombosListUpdated() {
			List<ItemComboViewModel> viewModels = new List<ItemComboViewModel>();

			if (Model != null) {
				var res = Tab.Table.FastItems.Where(p => {
					var model = p.GetModel<ItemCombo>();
					var script = model == null ? "" : model.Script;
					return script == Model.Script;
				}).ToList();

				foreach (var tuple in res) {
					viewModels.Add(new ItemComboViewModel(tuple, Model));
				}
			}

			LinkedCombos.ClearAndAddRange(viewModels);
		}

		public void CopyLinkedCombos(List<ItemCombo> entries) => Copy<ItemCombo, ItemComboWriterYaml>(entries, (v, writer, b) => writer.WriteItemCombo(b, v));

		public void ChangeItemCombos(List<ItemCombo> entries, ListCommandMode mode) {
			throw new NotImplementedException();
		}
	}
}
