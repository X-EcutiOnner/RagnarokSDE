using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Database;
using SDE.Databases.Generic.SearchDescriptors;
using SDE.Editor.Database;
using SDE.Editor.SearchFeature;
using TokeiLibrary;
using TokeiLibrary.WPF;
using TokeiLibrary.WPF.Styles.ListView;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.SearchFeature {
	public enum SearchConditionMode {
		Or,
		And
	}

	public partial class SearchEngine {
		private Debouncer _executor = new Debouncer(0);

		public class SearchResult {
			public bool Success;
			public bool SetResults;
			public bool IsAttributeRestricted;
			public IEnumerable<ReadableTuple> Tuples;
			public List<ReadableTuple> Results;
			public SearchFeature.Condition Condition;
			public List<string> TextSearchWords;
			public bool IsCondition => TextSearchWords == null;
			public bool IsTextSearch => TextSearchWords != null;

			public static SearchResult MakeSuccess(List<ReadableTuple> results) {
				SearchResult r = new SearchResult();
				r.Success = true;
				r.SetResults = true;
				return r;
			}

			public void SetEndResults(List<ReadableTuple> results) {
				Results = results;
				SetResults = true;
			}
		}

		protected bool _filterEnabled = true;
		protected bool _ignoreFilter;

		public virtual void Filter(object sender) {
			_filter(sender, null);
		}

		public virtual void Filter(object sender, Action finished) {
			_filter(sender, finished);
		}

		public virtual void ClearFilter() {
			try {
				_filterEnabled = false;
				_itemsSearchSettings.InternalClear();

				foreach (var attribute in _states) {
					_itemsSearchSettings[attribute.Key] = attribute.Value;
				}

				if (_tbItemsRange != null)
					_tbItemsRange.Dispatch(p => p.Clear());

				if (_tbSearchItems != null)
					_tbSearchItems.Dispatch(p => p.Clear());
			}
			finally {
				_filterEnabled = true;
			}
		}

		public virtual void IgnoreFilterOnce() {
			_ignoreFilter = true;
		}

		protected virtual void _filter(object sender) {
			_filter(sender, null);
		}

		protected virtual void _filter(object sender, Action finished) {
			if (!_filterEnabled) return;

			string currentSearch = _searchItemsFilter;
			IsFiltering = true;
			_validateLoaded();
			_executor.Execute(() => _filterInternal(currentSearch, finished));
			//GrfThread.Start(() => _filterInternal(currentSearch, finished), "SDEditor - Search filter items thread");
		}

		protected virtual void _filterInternal(string currentSearch, Action finished) {
			lock (_filterLock) {
				// This property, IsFiltering, needs to be refactored, it's currently used to indicate that a search is currently being done.
				// But more importantly, it's only used to wait for all items to be shown from the SelectionEngine attempting to
				// focus on an element.
				IsFiltering = true;

				try {
					if (currentSearch != _searchItemsFilter) return;
					if (_items == null) return;

					var inputResult = ProcessInput(currentSearch);

					if (inputResult.SetResults) {
						SetSearchResults(inputResult.Results);
						return;
					}

					if (currentSearch != _searchItemsFilter) return;

					List<Func<ReadableTuple, string, bool>> predicates = new List<Func<ReadableTuple, string, bool>>();

					if (inputResult.IsCondition) {
						SetTextBoxState(TextBoxState.Condition);
						predicates = GenerateConditionalPredicate(inputResult);
					}
					// Also conditional search, but the parsing failed midway, do a compromise search
					else if (inputResult.TextSearchWords.Any(p => p.StartsWith("[", StringComparison.Ordinal) && p.EndsWith("]", StringComparison.Ordinal))) {
						predicates = GeneratePartialConditionalPredicate(inputResult);
					}
					else {
						predicates = GenerateTextWordsPredicatesFromAllowedFields();
					}

					ApplyTupleSearchPredicates(ref inputResult.Tuples);

					if (currentSearch != _searchItemsFilter) return;

					var results = FetchSearchResults(inputResult, predicates);

					if (currentSearch != _searchItemsFilter) {
						SetTextBoxState(TextBoxState.Ok);
						return;
					}

					SetSearchResults(results);

					if (!inputResult.IsCondition)
						SetTextBoxState(TextBoxState.Ok);
				}
				catch {
					SetTextBoxState(TextBoxState.Ok);
				}
				finally {
					Utilities.Debug.Ignore(() => finished?.Invoke());
					IsFiltering = false;
				}
			}
		}

		public void ApplyTupleSearchPredicates(ref IEnumerable<ReadableTuple> tuples) {
			Func<ReadableTuple, bool> tuplePredicate = null;

			IList enumList;
			
			if (_searchDescriptor != null)
				enumList = _searchFields.Where(p => p.EnumType != null && _itemsSearchSettings[p]).ToList();
			else
				enumList = _attributes.Where(p => p.DataType.BaseType == typeof(Enum) && _itemsSearchSettings[p]).ToList();

			bool hasTuplePredicates = _itemsSearchSettings[SearchSettings.TupleAdded] ||
										_itemsSearchSettings[SearchSettings.TupleModified] ||
										_itemsSearchSettings[SearchSettings.TupleRange] ||
										enumList.Count > 0;

			if (hasTuplePredicates) {
				string predicateSearch = _tbItemsRange.Dispatch(() => _tbItemsRange.Text);
				tuplePredicate = _getTuplePredicates(enumList, predicateSearch);
			}

			if (tuplePredicate != null)
				tuples = tuples.Where(tuplePredicate);
		}

		private List<Func<ReadableTuple, string, bool>> GenerateTextWordsPredicatesFromAllowedFields() {
			if (_searchDescriptor != null) {
				List<Func<ReadableTuple, string, bool>> predicates =
					(from searchField in _searchFields
					 where searchField != null
					 let searchFieldLocal = searchField
					 where _itemsSearchSettings[searchFieldLocal]
					 where searchField.EnumType == null && searchField.IsTuple == false
					 select new Func<ReadableTuple, string, bool>((p, s) => searchFieldLocal.Getter(p.GetValue(_settings.ModelAttribute)).ToString().IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)).ToList();

				if (_searchFields.Any(p => p.IsTuple)) {
					predicates.AddRange(
						(from searchField in _searchFields
						 where searchField != null && searchField.IsTuple
						 let searchFieldLocal = searchField
						 select new Func<ReadableTuple, string, bool>((p, s) => searchFieldLocal.Getter(p).ToString().IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)).ToList()
					);
				}

				return predicates;
			}
			else
				return
					(from attribute in _attributes
					 where attribute != null
					 let attributeLocal = attribute
					 where _itemsSearchSettings[attributeLocal]
					 where attribute.DataType.BaseType != typeof(Enum)
					 select new Func<ReadableTuple, string, bool>((p, s) => p.GetValue<string>(attributeLocal).IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1)).ToList();
		}

		private List<Func<ReadableTuple, string, bool>> GeneratePartialConditionalPredicate(SearchResult inputResult) {
			List<Func<ReadableTuple, string, bool>> predicates = new List<Func<ReadableTuple, string, bool>>();
			var search = inputResult.TextSearchWords;

			for (int index = 0; index < search.Count; index++) {
				string se = search[index];
				int ival;

				if (se.StartsWith("[", StringComparison.Ordinal) && se.EndsWith("]", StringComparison.Ordinal)) {
					se = se.Substring(1, se.Length - 2);
					se = se.Replace("_", " ");
					var att = _settings.AttributeList.Attributes.FirstOrDefault(p => p.DisplayName.IndexOf(se, 0, StringComparison.OrdinalIgnoreCase) > -1);

					if (Int32.TryParse(se, out ival) || att != null) {
						if (ival < _settings.AttributeList.Attributes.Count) {
							DbAttribute attribute = att ?? _settings.AttributeList.Attributes[ival];
							inputResult.IsAttributeRestricted = true;
							string nextSearch = index + 1 < search.Count ? search[index + 1] : "";
							predicates.Add(new Func<ReadableTuple, string, bool>((p, s) => String.Compare(p.GetValue<string>(attribute), nextSearch, StringComparison.OrdinalIgnoreCase) == 0));
						}
						search.RemoveAt(index);
						index--;
					}
				}
			}

			return predicates;
		}

		public virtual List<Func<ReadableTuple, string, bool>> GenerateConditionalPredicate(SearchResult inputResult) {
			inputResult.Condition.ToPredicate(_settings, out var predicateSingle, out var predicateList);

			if (predicateList != null)
				return new List<Func<ReadableTuple, string, bool>>() { (s, t) => predicateList(s, t).Any() };
			else
				return new List<Func<ReadableTuple, string, bool>>() { predicateSingle };
		}

		public virtual SearchResult ProcessInput(string currentSearch) {
			List<ReadableTuple> tuples = GetTuples();
			SearchResult r = new SearchResult();
			r.Tuples = tuples;

			// There are no tuples in this table, just skip the search.
			if (tuples.Count == 0) {
				r.SetEndResults(new List<ReadableTuple>());
				return r;
			}

			// The _entryComparer is used to sort the results by the current sort used by the ListView.
			_items.Dispatch(p => _entryComparer.SetSort(ListViewExtensions.GetLastGetSearchAccessor(_items), ListViewExtensions.GetLastSortDirection(_items)));

			// This is used when the SelectionEngine focuses on an element that is not visible due to 
			// the current search, so this temporarily shows all tuples and ignores the current search entirely.
			if (_ignoreFilter) {
				r.SetEndResults(tuples);
				_ignoreFilter = false;
				return r;
			}

			// The search has two modes:
			// Direct text search, where all appropriate fields are compared to the list of words in the search filter. Ex: "archer and mage"
			// Condition search, where the search has conditions and looks through specific fields, and does logic comparisons.
			r.TextSearchWords = _getSearch(currentSearch, out r.Condition);

			// The search engine uses more than just its text field, it also has Enum restrictions and TupleAdded only restrictions, etc,
			// hence why it uses this condition instead of just checking for the text field.
			if (_isEmptySearch(r.TextSearchWords)) {
				r.SetEndResults(tuples);
				return r;
			}

			return r;
		}

		private bool _isEmptySearch(List<string> search) {
			if (_searchDescriptor != null)
				return search != null && search.Count == 0 &&
					!_searchFields.Where(p => p.EnumType != null).Any(p => _itemsSearchSettings[p]) &&
					!_itemsSearchSettings[SearchSettings.TupleAdded] &&
					!_itemsSearchSettings[SearchSettings.TupleModified] &&
					!_itemsSearchSettings[SearchSettings.TupleRange];
			else
				return search != null && search.Count == 0 &&
					!_attributes.Where(p => p.DataType.BaseType == typeof(Enum)).Any(p => _itemsSearchSettings[p]) &&
					!_itemsSearchSettings[SearchSettings.TupleAdded] &&
					!_itemsSearchSettings[SearchSettings.TupleModified] &&
					!_itemsSearchSettings[SearchSettings.TupleRange];
		}

		public virtual void SetSearchResults(IEnumerable<ReadableTuple> searchResult) {
			var results = searchResult.OrderBy(p => p, _entryComparer).ToList();
			_items.Dispatch(r => r.ItemsSource = new RangeObservableCollection<ReadableTuple>(results));
			SetTextBoxState(TextBoxState.Ok);
			OnFilterFinished(results);
		}

		public virtual List<ReadableTuple> GetTuples() {
			List<ReadableTuple> allItems = _getItemsFunction();

			if (SetupImageDataGetter != null) {
				allItems.Where(p => p.GetImageData == null).ToList().ForEach(p => p.GetImageData = SetupImageDataGetter);
			}

			return allItems;
		}

		public enum TextBoxState {
			Ok,
			Condition,
		}

		public virtual void SetTextBoxState(TextBoxState state) {
			_tbSearchItems.Dispatch(delegate {
				switch (state) {
					case TextBoxState.Ok:
						_tbSearchItems.Background = Application.Current.Resources["GSearchEngineOk"] as Brush;
						break;
					case TextBoxState.Condition:
						_tbSearchItems.Background = Application.Current.Resources["GSearchEnginePredicate"] as Brush;
						break;
				}
			});
		}

		protected virtual IEnumerable<ReadableTuple> FetchSearchResults(SearchResult inputResult, ICollection<Func<ReadableTuple, string, bool>> generalPredicates) {
			var search = inputResult.TextSearchWords;
			var tuples = inputResult.Tuples;

			// Search being null means this is a predicate-only search (such as: [Defense] == 5), it does not use keywords.
			if (inputResult.IsCondition || search == null) {
				return tuples.Where(item => generalPredicates.All(predicate => predicate(item, null)));
			}

			var conditionMode = _itemsSearchSettings.Get(SearchSettings.Mode) == "0" ? SearchConditionMode.Or : SearchConditionMode.And;

			// This is used for direct attribute mapping, such as [0] 501; though it's not used anymore because the whole project was moved to a model-based architecture.
			if (inputResult.IsAttributeRestricted && generalPredicates.Count != 0) {
				if (conditionMode == SearchConditionMode.Or)
					return tuples.Where(item => generalPredicates.Any(predicate => predicate(item, null)));
				return tuples.Where(item => generalPredicates.All(predicate => predicate(item, null)));
			}

			if (generalPredicates.Count == 0)
				return tuples;

			bool isSearchEmpty = search.Count == 0;

			if (isSearchEmpty)
				return tuples;

			if (conditionMode == SearchConditionMode.Or)
				return tuples.Where(item => search.Any(searchWord => generalPredicates.Any(predicate => predicate(item, searchWord))));
			return tuples.Where(item => search.All(searchWord => generalPredicates.Any(predicate => predicate(item, searchWord))));
		}

		protected virtual Func<ReadableTuple, bool> _getTuplePredicates(IList enumList, string predicateSearch) {
			Func<ReadableTuple, bool> tuplePredicate;

			List<Func<ReadableTuple, bool>> tuplePredicates = new List<Func<ReadableTuple, bool>>();

			if (_itemsSearchSettings[SearchSettings.TupleAdded])
				tuplePredicates.Add(new Func<ReadableTuple, bool>(item => item.Added));

			if (_itemsSearchSettings[SearchSettings.TupleModified])
				tuplePredicates.Add(new Func<ReadableTuple, bool>(item => item.Modified));

			List<Func<ReadableTuple, bool>> tupleTypePredicates;

			if (enumList is IEnumerable<DbAttribute> enumAttributes) {
				tupleTypePredicates =
					(from attributeCopy in enumAttributes
						let val = (int)attributeCopy.AttachedAttribute
						select new Func<ReadableTuple, bool>(item => (int)item.GetValue(attributeCopy) == val)).ToList();
			}
			else if (enumList is IEnumerable<SearchField> searchFields) {
				tupleTypePredicates =
					(from searchField in searchFields
					 let val = searchField.ActiveEnum
					 select new Func<ReadableTuple, bool>(item => searchField.Getter(item.GetValue(_settings.ModelAttribute)).Equals(val))).ToList();
			}
			else {
				throw new Exception("Invalid enumList type.");
			}

			List<Func<ReadableTuple, bool>> tupleRangePredicates = new List<Func<ReadableTuple, bool>>();

			if (_itemsSearchSettings[SearchSettings.TupleRange])
				tupleRangePredicates = GetRangePredicates(predicateSearch);

			if (tupleTypePredicates.Count > 0) {
				if (_itemsSearchSettings[SearchSettings.TupleRange] && tuplePredicates.Count > 0)
					tuplePredicate = new Func<ReadableTuple, bool>(item => tupleRangePredicates.Any(q => q(item)) && tuplePredicates.Any(q => q(item)) && tupleTypePredicates.All(q => q(item)));
				else if (_itemsSearchSettings[SearchSettings.TupleRange])
					tuplePredicate = new Func<ReadableTuple, bool>(item => tupleRangePredicates.Any(q => q(item)) && tupleTypePredicates.All(q => q(item)));
				else if (tuplePredicates.Count == 0)
					tuplePredicate = new Func<ReadableTuple, bool>(item => tupleTypePredicates.All(q => q(item)));
				else
					tuplePredicate = new Func<ReadableTuple, bool>(item => tuplePredicates.Any(q => q(item)) && tupleTypePredicates.All(q => q(item)));
			}
			else {
				if (_itemsSearchSettings[SearchSettings.TupleRange] && tuplePredicates.Count > 0)
					tuplePredicate = new Func<ReadableTuple, bool>(item => tupleRangePredicates.Any(q => q(item)) && tuplePredicates.Any(q => q(item)));
				else if (_itemsSearchSettings[SearchSettings.TupleRange])
					tuplePredicate = new Func<ReadableTuple, bool>(item => tupleRangePredicates.Any(q => q(item)));
				else
					tuplePredicate = new Func<ReadableTuple, bool>(item => tuplePredicates.Any(q => q(item)));
			}

			return tuplePredicate;
		}

		protected readonly string[] _symbols = { " <= ", " < ", " > ", " >= ", " = ", " == ", " != ", " ~= ", "!( ", " not(", " & ", " | ", " << ", " >> ", " % ", " * ", " / ", " ^ ", " - ", " contains ", " exclude ", " ⊃ ", " ⊅ " };

		protected virtual List<string> _getSearch(string currentSearch, out Condition condition) {
			condition = null;

			if (_symbols.Any(currentSearch.Contains)) {
				// Parse the expression is a condition
				try {
					var currentSearch2 = currentSearch
						.Replace(" and ", " && ")
						.Replace(" or ", " || ")
						.Replace(" != ", " ~= ")
						.Replace(" = ", " == ")
						.Replace(" contains ", " ⊃ ")
						.Replace(" exclude ", " ⊅ ")
						.Replace("!(", "not(");

					condition = ConditionLogic.GetCondition(currentSearch2);
					return null;
				}
				catch {
				}
			}

			List<string> search = new List<string>();
			string tempSearch = currentSearch;

			if (tempSearch.Contains('\"')) {
				int indexStart;
				int indexEnd;
				while (true) {
					indexStart = tempSearch.IndexOf('\"');

					if (indexStart < 0)
						break;

					indexEnd = tempSearch.IndexOf('\"', indexStart + 1);

					if (indexEnd < 0)
						break;

					if (indexStart + 1 == indexEnd)
						break;

					search.Add(tempSearch.Substring(indexStart + 1, indexEnd - indexStart - 1));
					tempSearch = tempSearch.Substring(0, indexStart) + tempSearch.Substring(indexEnd + 1, tempSearch.Length - indexEnd - 1);
				}
			}

			search.AddRange(tempSearch.ReplaceAll("  ", " ").Replace("\"", "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList());
			return search;
		}
	}
}