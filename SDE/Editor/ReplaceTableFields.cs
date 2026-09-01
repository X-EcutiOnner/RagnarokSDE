using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Database;
using ErrorManager;
using GRF.FileFormats;
using SDE.ApplicationConfiguration;
using SDE.Databases;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using TokeiLibrary;
using TokeiLibrary.WPF;

namespace SDE.Editor {
	public class FieldMapper {
		public AttributeList Get(DataSource source) {
			var res = _allTables.Values.FirstOrDefault(p => p.Source == source);

			if (res == null)
				throw new Exception("No table found which uses the DataSources [" + source + "]");

			return res.AttributeList;
		}

		public BaseDatabase GetCopyDb<TKey>(DataSource source) {
			var res = _allTables.Values.FirstOrDefault(p => p.Source == source);

			if (res == null)
				throw new Exception("No table found which uses the DataSources [" + source + "]");

			return res.Copy();
		}

		public DataSource SourceDestination;
		public DataSource SourceImport;

		// Leave null to copy all fields
		public DbAttribute[] Mapping;
		public int LastMappedIndex;
		private readonly Dictionary<DataSource, BaseDatabase> _allTables;
		private bool _allowCutLine = true;

		public FieldMapper(int maxElements, Dictionary<DataSource, BaseDatabase> allTables) {
			LastMappedIndex = maxElements;
			_allTables = allTables;
		}

		public DbAttribute[] GetMappingFields() {
			OkCancelEmptyDialog dialog = new OkCancelEmptyDialog("Fields to import", "treeList.png");
			dialog.Owner = WpfUtilities.TopWindow;

			Grid gridCopy = new Grid();
			gridCopy.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
			gridCopy.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });
			gridCopy.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(-1, GridUnitType.Auto) });

			gridCopy.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });

			for (int i = 0; i < 30; i++) {
				gridCopy.RowDefinitions.Add(new RowDefinition { Height = new GridLength(-1, GridUnitType.Auto) });
			}

			gridCopy.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });

			List<DbAttribute> attributes = new List<DbAttribute>();

			if (Mapping == null)
				attributes = Get(SourceImport).Attributes;
			else {
				for (int i = 0; i < Mapping.Length; i += 2) {
					attributes.Add(Mapping[i]);
				}
			}

			dialog.ContentControl.Content = gridCopy;

			int index = 0;
			List<CheckBox> boxes = new List<CheckBox>();

			foreach (DbAttribute attribute in attributes) {
				if (attribute.Index == 0) continue;
				if ((attribute.Visibility & VisibleState.Hidden) == VisibleState.Hidden) break;
				if (LastMappedIndex > -1 && attribute.Index >= LastMappedIndex) break;

				CheckBox box = new CheckBox { Margin = new Thickness(3, 3, 10, 3) };
				box.Content = attribute.DisplayName ?? attribute.AttributeName;
				box.Tag = attribute;
				box.SetValue(Grid.RowProperty, index / gridCopy.ColumnDefinitions.Count + 1);
				box.SetValue(Grid.ColumnProperty, index % gridCopy.ColumnDefinitions.Count);
				WpfUtilities.AddMouseInOutUnderline(box);

				gridCopy.Children.Add(box);
				boxes.Add(box);
				index++;
			}

			if (dialog.ShowDialog() == true) {
				if (boxes.All(p => p.IsChecked == false)) throw new OperationCanceledException();

				List<DbAttribute> mapping = new List<DbAttribute>();

				foreach (CheckBox box in boxes.Where(p => p.IsChecked == true)) {
					if (Mapping == null) {
						mapping.Add(box.Tag as DbAttribute);
						mapping.Add(box.Tag as DbAttribute);
					}
					else {
						for (int i = 0; i < Mapping.Length; i += 2) {
							if (Mapping[i] == box.Tag) {
								mapping.Add(box.Tag as DbAttribute);
								mapping.Add(Mapping[i + 1]);
							}
						}
					}
				}

				return mapping.ToArray();
			}
			throw new OperationCanceledException();
		}

		public void Map<TKey>(DbTab tab, string file, DbAttribute[] mappedFields) {
			//AbstractDb<TKey> db = GetCopyDb<TKey>(DbImport);
			//db.DummyInit(tab.ProjectDatabase);
			//
			//if (db.DbSource == ServerDbs.ClientItem) {
			//	if (file.IsExtension(".lua", ".lub")) {
			//		ClientItemReaderLua reader = new ClientItemReaderLua();
			//		db.DbLoader = (d, idb) => reader.LoadFile((AbstractDb<int>)(object)db, file);
			//	}
			//	else {
			//		throw new NotImplementedException();
			//		//db.DbLoader = (d, idb) => DbClientItemParser.LoadData((AbstractDb<int>)(object)db, file, mappedFields[0], _allowCutLine);
			//	}
			//}
			//
			//var method = db.DbLoader;
			//db.DbLoader = (d, idb) => {
			//	db.Table.EnableRawEvents = false;
			//	method(d, idb);
			//};
			//
			//try {
			//	if ((tab.DbComponent.DbSource & ServerDbs.ClientItem) != ServerDbs.ClientItem) {
			//		DebugStreamReader.ToServerEncoding = true;
			//	}
			//	else {
			//		DebugStreamReader.ToClientEncoding = true;
			//	}
			//
			//	db.LoadFromClipboard(file);
			//}
			//finally {
			//	DebugStreamReader.ToServerEncoding = false;
			//	DebugStreamReader.ToClientEncoding = false;
			//}
			//
			//var table = tab.Table;
			//
			//table.Commands.Begin();
			//
			//try {
			//	foreach (var cTuple in db.Table.FastItems) {
			//		var sTuple = table.TryGetTuple(cTuple.Key);
			//		if (sTuple == null) continue;
			//
			//		for (int i = 0; i < mappedFields.Length; i += 2) {
			//			var cValue = cTuple.GetValue<string>(mappedFields[i]);
			//			var sValue = sTuple.GetValue<string>(mappedFields[i + 1]);
			//
			//			if (cValue != sValue) {
			//				if (_isNoEmptyFields(mappedFields[i + 1], cValue, sValue)) continue;
			//				table.Commands.Set(sTuple, mappedFields[i + 1], cValue);
			//			}
			//		}
			//	}
			//}
			//catch (Exception err) {
			//	ErrorHandler.HandleException(err);
			//	table.Commands.CancelEdit();
			//}
			//finally {
			//	table.Commands.EndEdit();
			//}
		}

		private static bool _isNoEmptyFields(DbAttribute attribute, string cValue, string sValue) {
			//if (String.IsNullOrEmpty(cValue)) {
			//	if (NoEmptyFields.Contains(attribute)) return true;
			//}
			//
			//if (ZeroEmptyFields.Contains(attribute)) {
			//	int cVal;
			//	int sVal;
			//
			//	cValue = cValue ?? "";
			//	sValue = sValue ?? "";
			//
			//	Int32.TryParse(cValue, out cVal);
			//	Int32.TryParse(sValue, out sVal);
			//
			//	if (cVal == sVal) return true;
			//}

			return false;
		}

		//public static List<DbAttribute> NoEmptyFields = new List<DbAttribute> { ItemAttributes.Name, ClientItemAttributes.IdentifiedDisplayName, ClientItemAttributes.UnidentifiedDisplayName, ClientItemAttributes.Illustration, ClientItemAttributes.Affix, ClientItemAttributes.IdentifiedDescription, ClientItemAttributes.UnidentifiedDescription };
		//public static List<DbAttribute> ZeroEmptyFields = new List<DbAttribute> { ItemAttributes.ClassNumber, ClientItemAttributes.ClassNumber, ItemAttributes.NumberOfSlots, ClientItemAttributes.NumberOfSlots };

		public bool AllowCutLine {
			get { return _allowCutLine; }
			set { _allowCutLine = value; }
		}
	}

	public class ReplaceTableFields {
		public static void ReplaceFields(DbTab tab) {
			try {
				string[] files = PathRequest.OpenFilesCde("filter", FileFormat.MergeFilters(FileFormat.All, FileFormat.Txt, FileFormat.Lua));

				if (files == null || files.Length == 0) return;

				foreach (var file in files) {
					_readFile(file, tab);
				}
			}
			catch (OperationCanceledException) {
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}

		private static void _readFile(string file, DbTab tab) {
			try {
				//FieldMapper fieldMapper = new FieldMapper(SdeEditor.Project.GetDb(tab.Settings.Source).TabGenerator.MaxElementsToCopyInCustomMethods, SdeEditor.Project.AllTables);
				//fieldMapper.SourceDestination = tab.Settings.Source;

				//if ((tab.Settings.DbData & ServerDbs.AllItemTables) != 0) {
				//	if (file.IsExtension(".lua", ".lub")) {
				//		fieldMapper.DbImport = ServerDbs.ClientItems;
				//
				//		if ((tab.Settings.DbData & ServerDbs.ServerItems) != 0) {
				//			fieldMapper.Mapping = new DbAttribute[] {
				//				ClientItemAttributes.IdentifiedDisplayName, ItemAttributes.Name,
				//				ClientItemAttributes.UnidentifiedDisplayName, ItemAttributes.Name,
				//				ClientItemAttributes.ClassNumber, ItemAttributes.ClassNumber,
				//				ClientItemAttributes.NumberOfSlots, ItemAttributes.NumberOfSlots
				//			};
				//		}
				//		else {
				//			fieldMapper.Mapping = new DbAttribute[] {
				//				ClientItemAttributes.IdentifiedDisplayName, ClientItemAttributes.IdentifiedDisplayName,
				//				ClientItemAttributes.IdentifiedDescription, ClientItemAttributes.IdentifiedDescription,
				//				ClientItemAttributes.IdentifiedResourceName, ClientItemAttributes.IdentifiedResourceName,
				//				ClientItemAttributes.UnidentifiedDisplayName, ClientItemAttributes.UnidentifiedDisplayName,
				//				ClientItemAttributes.UnidentifiedDescription, ClientItemAttributes.UnidentifiedDescription,
				//				ClientItemAttributes.UnidentifiedResourceName, ClientItemAttributes.UnidentifiedResourceName,
				//				ClientItemAttributes.ClassNumber, ClientItemAttributes.ClassNumber,
				//				ClientItemAttributes.NumberOfSlots, ClientItemAttributes.NumberOfSlots
				//			};
				//		}
				//	}
				//	else if (file.IsExtension(".txt", ".conf")) {
				//		if (file.Contains("item_db")) {
				//			fieldMapper.DbImport = ServerDbs.Items;
				//
				//			if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//				fieldMapper.Mapping = new DbAttribute[] {
				//					ItemAttributes.Name, ClientItemAttributes.IdentifiedDisplayName,
				//					ItemAttributes.ClassNumber, ClientItemAttributes.ClassNumber,
				//					ItemAttributes.NumberOfSlots, ClientItemAttributes.NumberOfSlots
				//				};
				//			}
				//		}
				//		else {
				//			fieldMapper.DbImport = ServerDbs.ClientItems;
				//
				//			if (file.Contains("idnum2itemdisplaynametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0)
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.IdentifiedDisplayName, ClientItemAttributes.IdentifiedDisplayName };
				//				else
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.IdentifiedDisplayName, ItemAttributes.Name };
				//			}
				//			else if (file.Contains("num2itemdisplaynametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0)
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.UnidentifiedDisplayName, ClientItemAttributes.UnidentifiedDisplayName };
				//				else
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.UnidentifiedDisplayName, ItemAttributes.Name };
				//			}
				//			else if (file.Contains("idnum2itemdesctable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.IdentifiedDescription, ClientItemAttributes.IdentifiedDescription };
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else if (file.Contains("num2itemdesctable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.UnidentifiedDescription, ClientItemAttributes.UnidentifiedDescription };
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else if (file.Contains("idnum2itemresnametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.IdentifiedResourceName, ClientItemAttributes.IdentifiedResourceName };
				//					fieldMapper.AllowCutLine = false;
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else if (file.Contains("num2itemresnametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.UnidentifiedResourceName, ClientItemAttributes.UnidentifiedResourceName };
				//					fieldMapper.AllowCutLine = false;
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else if (file.Contains("num2cardillustnametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.Illustration, ClientItemAttributes.Illustration };
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else if (file.Contains("cardprefixnametable")) {
				//				if ((tab.Settings.DbData & ServerDbs.ClientItems) != 0) {
				//					fieldMapper.Mapping = new DbAttribute[] { ClientItemAttributes.Affix, ClientItemAttributes.Affix };
				//				}
				//				else throw new Exception("File not supported.");
				//			}
				//			else throw new Exception("File not supported.");
				//		}
				//	}
				//	else {
				//		throw new Exception("File name not recognized (must be item_db or idnum2itemdisplaynametable.");
				//	}
				//
				//	fieldMapper.Map(tab, file, fieldMapper.GetMappingFields());
				//}
				//else {
				//	fieldMapper.DbImport = tab.Settings.DbData;
				//	fieldMapper.Map(tab, file, fieldMapper.GetMappingFields());
				//}
			}
			catch (OperationCanceledException) {
				throw new OperationCanceledException();
			}
			catch (Exception err) {
				ErrorHandler.HandleException(err);
			}
		}
	}
}