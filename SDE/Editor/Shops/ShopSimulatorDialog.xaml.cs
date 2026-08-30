using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ErrorManager;
using GRF.FileFormats.ActFormat;
using GrfToWpfBridge;
using GrfToWpfBridge.ActRenderer;
using GrfToWpfBridge.DrawingComponents;
using SDE.ApplicationConfiguration;
using SDE.Core.Avalon;
using SDE.Databases;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Common.Jobs;
using SDE.Databases.Generic.Features;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Items.Parser;
using SDE.Editor.Engines.PreviewEngine;
using SDE.View;
using TokeiLibrary;
using TokeiLibrary.Shortcuts;
using TokeiLibrary.WPF.Styles;
using TokeiLibrary.WpfBugFix;
using Utilities;
using Utilities.Extension;
using static SDE.Databases.Generic.Controls.MobSpriteImage;

namespace SDE.Editor.Shops {
	/// <summary>
	/// Interaction logic for ScriptEditDialog.xaml
	/// </summary>
	public partial class ShopSimulatorDialog : TkWindow {
		private ShopViewModel _viewModel;
		private Act _act;
		private EditableListController<ShopItemViewModel, ShopItem, ItemReaderYaml> _itemsList;

		private FrameRendererEditor _editor = new FrameRendererEditor();
		private FrameRendererConfiguration _config;

		public ShopSimulatorDialog()
			: base("Shop simulator", "editor.png", SizeToContent.Height, ResizeMode.NoResize) {
			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Owner = WpfUtilities.TopWindow;

			_viewModel = new ShopViewModel();
			DataContext = _viewModel;

			Binder.Bind(_cbColorZeny, () => SdeAppConfiguration.UseZenyColors, () => _viewModel.ConfigChanged());
			Binder.Bind(_cbDiscount, () => SdeAppConfiguration.UseDiscount, () => _viewModel.ConfigChanged());
			Binder.Bind(_cbUseViewId, () => SdeAppConfiguration.AlwaysUseViewId, _updateViewIdType);
			
			_shop.TextChanged += _shop_TextChanged;

			AvalonLoader.Load(_shop);
			WpfUtilities.AddMouseInOutUnderline(_cbColorZeny, _cbDiscount, _cbUseViewId);

			_helper = new PreviewHelper(new RangeListView(), SdeEditor.Project.GetDb(DataSources.Item));

			_initializeActRenderer();

			_itemsList = new EditableListController<ShopItemViewModel, ShopItem, ItemReaderYaml>(_lvItems,
				changeListFunc: _viewModel.ChangeItems,
				newModel: () => new ShopItem() { Item = "501", Price = "-1" },
				modes: new EditableListMode[] { EditableListMode.Delete, EditableListMode.New, EditableListMode.MoveUp, EditableListMode.MoveDown }
			);

			_gridColumnPrimary.Width = new GridLength(230 + SystemParameters.VerticalScrollBarWidth + 7);

			this.Loaded += delegate {
				this.MinHeight = this.ActualHeight + 10;
				this.MinWidth = this.ActualWidth;
				this.ResizeMode = ResizeMode.CanResize;
				SizeToContent = SizeToContent.Manual;
			};

			ApplicationShortcut.Link(ApplicationShortcut.Undo, () => _undo(), this);
			ApplicationShortcut.Link(ApplicationShortcut.Redo, () => _redo(), this);

			// Setting the text directly to the text editor will create a new model for the view model.
			_shop.Text = "alberta_in,182,97,4	shop	Tool Dealer#alb2	73,1750:750,611:-1,501:-1,502:-1,503:-1,504:-1,506:-1,645:-1,656:-1,601:-1,602:-1,2243:-1";
			_viewModel.ShopCodeUpdated += _viewModel_ShopCodeUpdated;
			_updateViewShop();
		}

		private void _updateViewIdType() {
			try {
				string viewId = _viewModel.NpcViewId;
				int ival;

				if (SdeAppConfiguration.AlwaysUseViewId) {
					if (!Int32.TryParse(viewId, out ival)) {
						JobType jobType = 0;

						if (DbReader.LoadEnum(ref jobType, viewId, false)) {
							_viewModel.NpcViewId = ((int)jobType).ToString(CultureInfo.InvariantCulture);
						}
					}
				}
				else {
					if (Int32.TryParse(viewId, out ival)) {
						viewId = ViewIdToString(ival);

						if (!String.IsNullOrEmpty(viewId)) {
							if (viewId.IsExtension(".act", ".spr")) {
								_viewModel.NpcViewId = Path.GetFileNameWithoutExtension(viewId.ToUpper());
							}
							else {
								_viewModel.NpcViewId = Path.GetFileName(viewId);
							}
						}
					}
				}
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private bool _enableEvents = true;

		private void _viewModel_ShopCodeUpdated(string newShopCode) {
			if (!_enableEvents) return;

			try {
				_enableEvents = false;
				_shop.Document.Replace(0, _shop.Document.TextLength, newShopCode);
				_updateViewShop();
			}
			finally {
				_enableEvents = true;
			}
		}

		private void _shop_TextChanged(object sender, EventArgs eventArgs) {
			if (!_enableEvents) return;

			try {
				_enableEvents = false;
				_viewModel.LoadFromShopCode(_shop.Text);
				_itemsList.SelectPrevious();
			}
			finally {
				_enableEvents = true;
			}
		}

		private void _undo() => Debug.Ignore(() => _shop.Undo());
		private void _redo() => Debug.Ignore(() => _shop.Redo());

		#region Preview Act
		private void _initializeActRenderer() {
			_editor.IndexSelector = _indexSelector;
			_editor.FrameRenderer = _renderer;
			_editor.GridPrimary = _gridActRenderer;

			_config = new FrameRendererConfiguration(SdeAppConfiguration.ConfigAsker);
			_indexSelector.Init(_editor, 0, 0, _config);

			_renderer.RelativeCenter = new Point(0.5d, 0.8d);
			_renderer.Init(_editor, _config);
			_renderer.Canvas.Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));

			_renderer.DrawingModules.Add(new DefaultDrawModule(delegate {
				if (_editor.Act != null) {
					return new List<DrawingComponent> { new ActDraw(_editor.Act) };
				}

				return new List<DrawingComponent>();
			}));

			_editor.FrameRenderer.GridZoom.VerticalAlignment = VerticalAlignment.Bottom;
			_editor.FrameRenderer.GridZoom.HorizontalAlignment = HorizontalAlignment.Left;
		}

		private void _setAnimation(Act act) {
			act?.Safe();

			this.Dispatch(delegate {
				_editor.IsLoading = true;
				_editor.Act = act;
				_editor.OnActLoaded();
				_editor.IndexSelector.Init(_editor, _editor.PreferedLoadingAction, 0);

				_editor.FrameRenderer.Update();
				_editor.IsLoading = false;
			});
		}
		#endregion

		public static string ViewIdToString(int viewId) {
			NpcPreview preview = new NpcPreview();

			_helper.ViewId = viewId;
			preview.Read(null, _helper, new List<Job>());

			return preview.GetSpriteFromJob(null, _helper);
		}

		private void _updateViewShop() {
			try {
				var viewIds = _viewModel.NpcViewId;
				var shopLocation = _viewModel.NpcPosition;

				var locations = shopLocation.Split(',');
				int dir = 0;
				int viewId = -1;

				if (locations.Length == 4) {
					Int32.TryParse(locations[3], out dir);
				}

				if (viewIds == "-1") {
				}
				else {
					if (!Int32.TryParse(viewIds, out viewId)) {
						JobType jobType = 0;

						if (DbReader.LoadEnum(ref jobType, viewIds, false)) {
							viewId = (int)jobType;
						}
						else {
							viewId = -1;
						}
					}
				}

				if (viewId < 0) {
					_act = null;
				}
				else {
					NpcPreview preview = new NpcPreview();

					_helper.ViewId = viewId;
					preview.Read(null, _helper, new List<Job>());

					var sprite = preview.GetSpriteFromJob(null, _helper);

					if (sprite.EndsWith(".act")) {
						var actData = SdeEditor.MetaGrf.GetData(sprite);
						var sprData = SdeEditor.MetaGrf.GetData(sprite.ReplaceExtension(".spr"));

						if (actData != null && sprData != null) {
							_act = new Act(actData, sprData);
						}
						else {
							_act = null;
						}
					}
					else {
						_act = null;
					}
				}

				_setAnimation(_act);
				_indexSelector.SelectedAction = _convertAction(dir);
			}
			catch {
				//ErrorHandler.HandleException(err);
			}
		}

		private int _convertAction(int action) {
			switch (action % 8) {
				case 0: return 4;
				case 1: return 3;
				case 2: return 2;
				case 3: return 1;
				case 4: return 0;
				case 5: return 7;
				case 6: return 6;
				case 7: return 5;
				default: return -1;
			}
		}

		private static PreviewHelper _helper;

		private void _lvItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			_viewModel.SelectedItem = _lvItems.SelectedItem as ShopItemViewModel;

			_itemsList.SaveSelection();
			_itemsList.SelectPrevious();
			//Core.Extensions.ClearUndos(_gridTargets);
		}

		protected override void GRFEditorWindowKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Close();
		}

		private void _buttonCancel_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void _tbPriceReset_Click(object sender, RoutedEventArgs e) {
			_viewModel.SelectedItem.Price = "-1";
		}
	}
}
