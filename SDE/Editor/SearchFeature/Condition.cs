using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GRF.FileFormats.LubFormat;
using Microsoft.Scripting.Utils;
using SDE.Databases.Generic.Common;
using SDE.Editor.Database;
using SDE.Editor.Generic.DbTabs;
using Utilities;
using Utilities.Extension;

namespace SDE.Editor.SearchFeature {
	public class BooleanCondition : Condition {
		public bool Value { get; set; }

		public BooleanCondition(string value) {
			if (value == "true")
				Value = true;
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			Value = !Value;
		}

		protected override string _getStringValue() {
			return Value ? "true" : "false";
		}

		public override Condition Copy() {
			return new BooleanCondition(Value.ToString()) { Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => Value);
			predicateList = null;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			ToPredicate(settings, out var predicate, out _);
			predicateSingle = (t, s) => predicate(t, s).ToString();
			predicateList = null;
		}
	}

	public class RelationalCondition : Condition {
		private RelationalComparison _comparison = RelationalComparison.None;
		private Condition _leftCondition;
		private Condition _rightCondition;

		public RelationalCondition(Condition left, RelationalComparison comparison, Condition right) {
			left = ConditionLogic.GetCondition(left);
			right = ConditionLogic.GetCondition(right);

			_leftCondition = left;
			_rightCondition = right;
			_comparison = comparison;

			Suffix = left.Suffix;
			Prefix = left.Prefix;

			left.Prefix = "";
			left.Suffix = "";
			right.Prefix = "";
			right.Suffix = "";

			RelationalCondition rLeft = left as RelationalCondition;
			if (rLeft != null) {
				RelationalComparison comp = rLeft._comparison;

				if ((comp == RelationalComparison.And || comp == RelationalComparison.Or) &&
				    (comparison == RelationalComparison.And || comparison == RelationalComparison.Or) &&
				    rLeft._comparison != comparison) {
					_leftCondition = new ParenthesisCondition(rLeft);
				}
			}

			RelationalCondition rRight = right as RelationalCondition;
			if (rRight != null) {
				RelationalComparison comp = rRight._comparison;

				if ((comp == RelationalComparison.And || comp == RelationalComparison.Or) &&
				    (comparison == RelationalComparison.And || comparison == RelationalComparison.Or) &&
				    rRight._comparison != comparison) {
					_rightCondition = new ParenthesisCondition(rRight);
				}
			}
		}

		public RelationalCondition(string left, string comparison, string right) {
			_init(left, comparison, right);
		}

		public RelationalCondition(string left, string rightSide) {
			_init(left, rightSide);
		}

		public RelationalCondition(string full) {
			// The left side will never have parenthesis
			full = full.Replace(" and ", "&&").Replace(" or ", "||");
			int startIndex = 0;

			while (!_isConditionCharacter(full[startIndex])) {
				startIndex++;
			}

			startIndex--;

			_init(full.Substring(0, startIndex), full.Substring(startIndex));
		}

		public override Condition Copy() {
			return new RelationalCondition(_leftCondition.Copy(), _comparison, _rightCondition.Copy()) { Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			predicateList = null;
			isInt = false;

			if (_comparison >= RelationalComparison.BinaryAnd) {
				isInt = true;
				bool int2;
				_leftCondition.ToDouble(settings, out var predicateLeftSingle, out var predicateLeftList, out int2);
				_rightCondition.ToDouble(settings, out var predicateRightSingle, out var predicateRightList, out int2);

				predicateSingle = new Func<ReadableTuple, string, double>((t, s) => {
					double left;

					if (predicateLeftList != null)
						left = predicateLeftList(t, s).First();
					else
						left = predicateLeftSingle(t, s);

					double right;

					if (predicateRightList != null)
						right = predicateRightList(t, s).First();
					else
						right = predicateRightSingle(t, s);

					switch (_comparison) {
						case RelationalComparison.BinaryAnd:
							return (long)left & (long)right;
						case RelationalComparison.BinaryOr:
							return (long)left | (long)right;
						case RelationalComparison.BinaryRightShift:
							return (long)left >> (int)right;
						case RelationalComparison.BinaryLeftShift:
							return (long)left << (int)right;
						case RelationalComparison.Add:
							return left + right;
						case RelationalComparison.Minus:
							return left - right;
						case RelationalComparison.Mult:
							return left * right;
						case RelationalComparison.Div:
							return left / right;
						case RelationalComparison.Mod:
							return left % right;
						case RelationalComparison.Pow:
							return Math.Pow(left, right);
					}

					return 0;
				});

				return;
			}

			predicateSingle = new Func<ReadableTuple, string, double>((t, s) => 0);
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			Func<ReadableTuple, string, bool> leftBool = (t, s) => false;
			Func<ReadableTuple, string, bool> rightBool = (t, s) => false;
			Func<ReadableTuple, string, double> leftDouble = (t, s) => 0;
			Func<ReadableTuple, string, double> rightDouble = (t, s) => 0;
			Func<ReadableTuple, string, string> leftString = (t, s) => "";
			Func<ReadableTuple, string, string> rightString = (t, s) => "";

			Func<ReadableTuple, string, List<bool>> leftBoolList = null;
			Func<ReadableTuple, string, List<bool>> rightBoolList = null;
			Func<ReadableTuple, string, List<double>> leftDoubleList = null;
			Func<ReadableTuple, string, List<double>> rightDoubleList = null;
			Func<ReadableTuple, string, List<string>> leftStringList = null;
			Func<ReadableTuple, string, List<string>> rightStringList = null;

			bool isLeftInt = false;
			bool isRightInt = false;

			predicateList = null;

			if (_leftCondition != null) {
				_leftCondition.ToPredicate(settings, out leftBool, out leftBoolList);
				_leftCondition.ToDouble(settings, out leftDouble, out leftDoubleList, out isLeftInt);
				_leftCondition.ToValue(settings, out leftString, out leftStringList);
			}

			if (_rightCondition != null) {
				_rightCondition.ToPredicate(settings, out rightBool, out rightBoolList);
				_rightCondition.ToDouble(settings, out rightDouble, out rightDoubleList, out isRightInt);
				_rightCondition.ToValue(settings, out rightString, out rightStringList);
			}

			switch (_comparison) {
				case RelationalComparison.And:
				case RelationalComparison.Or:
					predicateSingle = (t, s) => {
						bool left = leftBoolList != null ? leftBoolList(t, s).Any(p => p) : leftBool(t, s);
						bool right = rightBoolList != null ? rightBoolList(t, s).Any(p => p) : rightBool(t, s);

						if (_comparison == RelationalComparison.And)
							return left && right;
						else
							return left || right;
					};
					return;
				case RelationalComparison.Contains:
				case RelationalComparison.Exclude:
					predicateSingle = (t, s) => {
						if (isLeftInt || isRightInt) {
							var left = leftDoubleList != null
								? leftDoubleList(t, s).Select(p => p.ToString(CultureInfo.InvariantCulture))
								: new[] { leftDouble(t, s).ToString(CultureInfo.InvariantCulture) };
							
							var right = rightDoubleList != null
								? rightDoubleList(t, s).Select(p => p.ToString(CultureInfo.InvariantCulture))
								: new[] { rightDouble(t, s).ToString(CultureInfo.InvariantCulture) };

							bool AnyMatch() => left.Any(l => right.Any(r => l.IndexOf(r, StringComparison.OrdinalIgnoreCase) > -1));

							return _comparison == RelationalComparison.Contains ? AnyMatch() : !AnyMatch();
						}
						else {
							var left = leftStringList != null
								? leftStringList(t, s).Select(p => p)
								: new[] { leftString(t, s) };

							var right = rightStringList != null
								? rightStringList(t, s).Select(p => p)
								: new[] { rightString(t, s) };

							bool AnyMatch() => left.Any(l => right.Any(r => l.IndexOf(r, StringComparison.OrdinalIgnoreCase) > -1));

							return _comparison == RelationalComparison.Contains ? AnyMatch() : !AnyMatch();
						}
					};
					return;
				case RelationalComparison.Eq:
				case RelationalComparison.NotEq:
					predicateSingle = (t, s) => {
						if (isLeftInt || isRightInt) {
							var left = leftDoubleList != null
								? leftDoubleList(t, s).Select(p => p.ToString(CultureInfo.InvariantCulture))
								: new[] { leftDouble(t, s).ToString(CultureInfo.InvariantCulture) };

							var right = rightDoubleList != null
								? rightDoubleList(t, s).Select(p => p.ToString(CultureInfo.InvariantCulture))
								: new[] { rightDouble(t, s).ToString(CultureInfo.InvariantCulture) };

							bool Match() => left.Any(l => right.Any(r => l == r));

							return _comparison == RelationalComparison.Eq ? Match() : !Match();
						}
						else {
							var left = leftStringList != null
								? leftStringList(t, s).Select(p => p)
								: new[] { leftString(t, s) };

							var right = rightStringList != null
								? rightStringList(t, s).Select(p => p)
								: new[] { rightString(t, s) };

							bool Match() => left.Any(l => right.Any(r => string.Compare(l, r, StringComparison.OrdinalIgnoreCase) == 0));

							return _comparison == RelationalComparison.Eq ? Match() : !Match();
						}
					};
					return;
				case RelationalComparison.Ge:
				case RelationalComparison.Le:
				case RelationalComparison.Gt:
				case RelationalComparison.Lt:
					predicateSingle = (t, s) => {
						var left = leftDoubleList != null
								? leftDoubleList(t, s).Select(p => p)
								: new[] { leftDouble(t, s) };

						var right = rightDoubleList != null
							? rightDoubleList(t, s).Select(p => p)
							: new[] { rightDouble(t, s) };

						switch (_comparison) {
							case RelationalComparison.Ge:
								return left.Any(l => right.Any(r => l >= r));
							case RelationalComparison.Le:
								return left.Any(l => right.Any(r => l <= r));
							case RelationalComparison.Gt:
								return left.Any(l => right.Any(r => l > r));
							case RelationalComparison.Lt:
								return left.Any(l => right.Any(r => l < r));
						}

						return false;
					};
					return;
			}

			predicateSingle = (t, s) => false;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			ToPredicate(settings, out var predicate, out _);
			predicateSingle = (t, s) => predicate(t, s).ToString();
			predicateList = null;
		}

		private void _init(string left, string rightSide) {
			rightSide = rightSide.Replace(" and ", "&&").Replace(" or ", "||");
			int startIndex = 0;

			while (_isConditionCharacter(rightSide[startIndex])) {
				startIndex++;
			}

			startIndex--;

			_init(left, rightSide.Substring(0, startIndex), rightSide.Substring(startIndex));
		}

		private void _init(string left, string comparison, string right) {
			_leftCondition = ConditionLogic.GetCondition(left);
			_readComparison(comparison);
			_rightCondition = ConditionLogic.GetCondition(right);
		}

		private void _readComparison(string comparison) {
			string condition = comparison.Replace("and", "&&").Replace("or", "||").Trim(' ');

			switch(condition) {
				case "<":
					_comparison = RelationalComparison.Lt;
					break;
				case ">":
					_comparison = RelationalComparison.Gt;
					break;
				case "<=":
					_comparison = RelationalComparison.Le;
					break;
				case ">=":
					_comparison = RelationalComparison.Ge;
					break;
				case "==":
					_comparison = RelationalComparison.Eq;
					break;
				case "⊃":
					_comparison = RelationalComparison.Contains;
					break;
				case "⊅":
					_comparison = RelationalComparison.Exclude;
					break;
				case "~=":
					_comparison = RelationalComparison.NotEq;
					break;
				case "!=":
					_comparison = RelationalComparison.NotEq;
					break;
				case "&&":
					_comparison = RelationalComparison.And;
					break;
				case "||":
					_comparison = RelationalComparison.Or;
					break;
				case "+":
					_comparison = RelationalComparison.Add;
					break;
				case "-":
					_comparison = RelationalComparison.Minus;
					break;
				case "*":
					_comparison = RelationalComparison.Mult;
					break;
				case "/":
					_comparison = RelationalComparison.Div;
					break;
				case "^":
					_comparison = RelationalComparison.Pow;
					break;
				case "%":
					_comparison = RelationalComparison.Mod;
					break;
				case "&":
					_comparison = RelationalComparison.BinaryAnd;
					break;
				case "|":
					_comparison = RelationalComparison.BinaryOr;
					break;
				case "~":
					_comparison = RelationalComparison.Not;
					break;
				case ">>":
					_comparison = RelationalComparison.BinaryRightShift;
					break;
				case "<<":
					_comparison = RelationalComparison.BinaryLeftShift;
					break;
			}
		}

		private bool _isConditionCharacter(char c) {
			return c == '=' || c == '<' || c == '>' || c == '~' || c == '&' || c == '|' || c == '+' || c == '/' || c == '*' || c == '-' || c == '%' || c == '^' || c == ' ';
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			deep--;

			switch(_comparison) {
				case RelationalComparison.And:
				case RelationalComparison.Or:
					_leftCondition.Reverse(deep);
					_comparison = _comparison == RelationalComparison.Or ? RelationalComparison.And : RelationalComparison.Or;
					_rightCondition.Reverse(deep);
					break;
				case RelationalComparison.Eq:
					_comparison = RelationalComparison.NotEq;
					break;
				case RelationalComparison.NotEq:
					_comparison = RelationalComparison.Eq;
					break;
				case RelationalComparison.Contains:
					_comparison = RelationalComparison.Exclude;
					break;
				case RelationalComparison.Exclude:
					_comparison = RelationalComparison.Contains;
					break;
				case RelationalComparison.Ge:
					_comparison = RelationalComparison.Lt;
					break;
				case RelationalComparison.Lt:
					_comparison = RelationalComparison.Ge;
					break;
				case RelationalComparison.Gt:
					_comparison = RelationalComparison.Le;
					break;
				case RelationalComparison.Le:
					_comparison = RelationalComparison.Gt;
					break;
			}
		}

		protected override string _getStringValue() {
			if (_comparison == RelationalComparison.Eq || _comparison == RelationalComparison.NotEq) {
				BooleanCondition bC;

				// Simplify cases, such as...
				// condition == true > condition
				// 
				if (_comparison == RelationalComparison.Eq) {
					Func<BooleanCondition, Condition, string> simplify = new Func<BooleanCondition, Condition, string>((condBool, cond2) => {
						if (condBool.Value) {
							return cond2;
						}
						cond2.Reverse();
						return cond2;
					});

					bC = _leftCondition as BooleanCondition;
					if (bC != null) return simplify(bC, _rightCondition);
					bC = _rightCondition as BooleanCondition;
					if (bC != null) return simplify(bC, _leftCondition);
				}
				else if (_comparison == RelationalComparison.NotEq) {
					Func<BooleanCondition, Condition, string> simplify = new Func<BooleanCondition, Condition, string>((condBool, cond2) => {
						if (!condBool.Value) {
							return cond2;
						}
						cond2.Reverse();
						return cond2;
					});

					bC = _leftCondition as BooleanCondition;
					if (bC != null) return simplify(bC, _rightCondition);
					bC = _rightCondition as BooleanCondition;
					if (bC != null) return simplify(bC, _leftCondition);
				}
			}

			return _leftCondition + RelationToString(_comparison) + _rightCondition;
		}

		public static string RelationToString(RelationalComparison comparison) {
			switch(comparison) {
				case RelationalComparison.None:
					return "";
				case RelationalComparison.Le:
					return " <= ";
				case RelationalComparison.Lt:
					return " < ";
				case RelationalComparison.Eq:
					return " == ";
				case RelationalComparison.NotEq:
					return " ~= ";
				case RelationalComparison.Contains:
					return " ⊃ ";
				case RelationalComparison.Exclude:
					return " ⊅ ";
				case RelationalComparison.Ge:
					return " >= ";
				case RelationalComparison.Gt:
					return " > ";
				case RelationalComparison.And:
					return " and ";
				case RelationalComparison.Or:
					return " or ";
				case RelationalComparison.Add:
					return " + ";
				case RelationalComparison.Minus:
					return " - ";
				case RelationalComparison.Mult:
					return " * ";
				case RelationalComparison.Div:
					return " / ";
				case RelationalComparison.Pow:
					return " ^ ";
				case RelationalComparison.Mod:
					return " % ";
				case RelationalComparison.BinaryAnd:
					return " & ";
				case RelationalComparison.BinaryOr:
					return " | ";
				case RelationalComparison.Not:
					return " ~";
				case RelationalComparison.BinaryRightShift:
					return " >> ";
				case RelationalComparison.BinaryLeftShift:
					return " << ";
				default:
					return "";
			}
		}
	}

	public class ParenthesisCondition : Condition {
		public bool IsReversed { get; private set; }
		public Condition Condition { get; set; }

		public ParenthesisCondition(string value) {
			Condition = ConditionLogic.GetCondition(value);
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			IsReversed = !IsReversed;
		}

		protected override string _getStringValue() {
			return IsReversed ? "not(" + Condition + ")" : "(" + Condition + ")";
		}

		public override Condition Copy() {
			return new ParenthesisCondition(Condition.Copy()) { IsReversed = IsReversed, Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			Condition.ToPredicate(settings, out var predicateSingleLocal, out var predicateListLocal);
			predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => {
				if (predicateListLocal != null) {
					return predicateListLocal(t, s).Any(p => IsReversed ? !p : p);
				}

				return IsReversed ? !predicateSingleLocal(t, s) : predicateSingleLocal(t, s);
			});
			predicateList = null;
		}

		public override void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			Condition.ToDouble(settings, out predicateSingle, out predicateList, out isInt);
			return;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			ToPredicate(settings, out var predicate, out _);
			predicateSingle = (t, s) => predicate(t, s).ToString();
			predicateList = null;
		}
	}

	public class VariableCondition : Condition {
		public bool IsReversed { get; private set; }
		public string Value { get; set; }

		public VariableCondition(string value) {
			for (int i = 0; i < value.Length; i++) {
				if (ConditionLogic.IsConditionCharacterWithoutSpace(value[i]))
					throw new Exception("Invalid character " + value[i]);
			}

			Value = value;
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			IsReversed = !IsReversed;
		}

		protected override string _getStringValue() {
			return IsReversed ? "not(" + Value + ")" : Value;
		}

		public override Condition Copy() {
			return new VariableCondition(Value) { IsReversed = IsReversed, Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			predicateList = null;

			if (Value.StartsWith("[") && Value.EndsWith("]")) {
				string se = Value.Substring(1, Value.Length - 2);

				if (settings.UseModel) {
					predicateSingle = null;
					predicateList = new Func<ReadableTuple, string, List<bool>>((t, s) => {
						var model = t.GetValue(settings.ModelAttribute);

						if (model == null)
							return new List<bool>() { false };

						var r = TypeTreeHelper.GetValue(model, se);

						if (r == null || r.Count == 0)
							return new List<bool>() { false };

						List<bool> ret = new List<bool>();

						foreach (var entry in r) {
							bool.TryParse((entry ?? "false").ToString(), out var bval);
							ret.Add(bval);
						}

						return ret;
					});

					return;
				}

				var att = settings.AttributeList.Find(se);

				if (att >= 0) {
					predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => {
						string val = t.GetValue<string>(att);
						bool ival2;
						bool.TryParse(val, out ival2);
						return ival2;
					});
					return;
				}

				predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => false);
				return;
			}

			bool.TryParse(Value, out bool b);
			predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => b);
		}

		public override void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			isInt = false;
			predicateSingle = null;
			predicateList = null;

			if (Value.StartsWith("[") && Value.EndsWith("]")) {
				string se = Value.Substring(1, Value.Length - 2);

				if (settings.UseModel) {
					predicateList = new Func<ReadableTuple, string, List<double>>((t, s) => {
						var model = t.GetValue(settings.ModelAttribute);

						if (model == null)
							return new List<double>() { 0 };

						var r = TypeTreeHelper.GetValue(model, se);

						if (r == null || r.Count == 0)
							return new List<double>() { 0 };

						return r.Select(val => {
							if (val == null)
								return 0.0;

							if (val is Enum enumValue)
								return (int)val;
							else if (val is int intValue)
								return intValue;
							else if (val is bool boolValue)
								return boolValue ? 1 : 0;

							string valueString = (val ?? "").ToString();
							return FormatConverters.LongOrHexConverter(valueString);
						}).ToList();
					});

					return;
				}

				var att = settings.AttributeList.Find(se);
				bool? canDirect = null;

				if (att >= 0) {
					predicateSingle = new Func<ReadableTuple, string, double>((t, s) => {
						if (canDirect == true) {
							try {
								return t.GetValue<int>(att);
							}
							catch {
								canDirect = false;
							}
						}

						string val = t.GetValue<string>(att);

						if (canDirect == null) {
							try {
								return t.GetValue<int>(att);
							}
							catch {
								canDirect = false;
							}
						}

						if (!long.TryParse(val, out long value)) {
							if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
								value = FormatConverters.LongOrHexConverter(val);
							}
						}

						return value;
					});

					return;
				}

				predicateSingle = new Func<ReadableTuple, string, double>((t, s) => 0);
				return;
			}

			double ival;
			if (double.TryParse(Value.Replace(".", ","), out ival)) {
				isInt = true;
			}
			else if (double.TryParse(Value.Replace(",", "."), out ival)) {
				isInt = true;
			}
			else {
				if (Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
					ival = FormatConverters.LongOrHexConverter(Value);
					isInt = true;
				}
			}

			predicateSingle = new Func<ReadableTuple, string, double>((t, s) => ival);
			return;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			predicateSingle = null;
			predicateList = null;

			if (Value.StartsWith("[") && Value.EndsWith("]")) {
				string se = Value.Substring(1, Value.Length - 2);

				if (settings.UseModel) {
					predicateList = new Func<ReadableTuple, string, List<string>>((t, s) => {
						var model = t.GetValue(settings.ModelAttribute);

						if (model == null)
							return new List<string>() { "" };

						var r = TypeTreeHelper.GetValue(model, se);

						if (r == null || r.Count == 0)
							return new List<string>() { "" };

						if (r.All(p => p is Enum)) {
							var dico = EnumInfos.GetEnumTypeToInfo(r[0].GetType());

							if (dico != null) {
								return r.Select(p => {
									//if (dico.TryGetValue((Enum)p, out var enumBase))
									//	return enumBase.YamlName;
									//else
										return p.ToString();
								}).ToList();
							}
						}

						return r.Select(p => (p ?? "").ToString()).ToList();
					});

					return;
				}

				var att = settings.AttributeList.Find(se);

				if (att >= 0) {
					predicateSingle = new Func<ReadableTuple, string, string>((t, s) => t.GetValue<string>(att));
					return;
				}
			}

			if (Value.StartsWith("\"") && Value.EndsWith("\"") && Value.Length >= 2) {
				Value = Value.Substring(1, Value.Length - 2);
			}

			predicateSingle = new Func<ReadableTuple, string, string>((t, s) => Value);
		}
	}

	public class NotCondition : Condition {
		public bool IsReversed { get; private set; }
		public Condition Condition { get; set; }

		public NotCondition(string value) {
			Condition = ConditionLogic.GetCondition(value);
		}

		public NotCondition(Condition value) {
			Condition = value;
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			IsReversed = !IsReversed;
		}

		protected override string _getStringValue() {
			NotCondition nC = Condition as NotCondition;
			if (nC != null) {
				if (!IsReversed && !nC.IsReversed) {
					return Condition;
				}
			}

			if (IsReversed && !(Condition is RelationalCondition)) {
				return Condition;
			}

			if (!IsReversed) {
				if (Condition is RelationalCondition || Condition is BooleanCondition)
					return Condition.Reverse();
			}

			return IsReversed ? "(" + Condition + ")" : "not(" + Condition + ")";
		}

		public override Condition Copy() {
			return new NotCondition(Condition.Copy()) { IsReversed = IsReversed, Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			Condition.ToPredicate(settings, out var predicateSingleLocal, out var predicateListLocal);
			predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => {
				if (predicateListLocal != null) {
					return predicateListLocal(t, s).Any(p => IsReversed ? p : !p);
				}

				return IsReversed ? predicateSingleLocal(t, s) : !predicateSingleLocal(t, s);
			});
			predicateList = null;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			predicateSingle = null;
			predicateList = null;

			ToPredicate(settings, out var predicateSingleLocal, out var predicateListLocal);

			if (predicateSingle != null)
				predicateSingle = (t, s) => predicateSingleLocal(t, s).ToString();

			if (predicateListLocal != null)
				predicateList = (t, s) => predicateListLocal(t, s).Select(p => p.ToString()).ToList();
		}

		public override void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			Condition.ToDouble(settings, out predicateSingle, out predicateList, out isInt);
			return;
		}
	}

	public class UnaryNotCondition : Condition {
		public bool IsReversed { get; private set; }
		public Condition Condition { get; set; }

		public UnaryNotCondition(string value) {
			Condition = ConditionLogic.GetCondition(value);
		}

		public UnaryNotCondition(Condition value) {
			Condition = value;
		}

		internal override void Reverse(int deep) {
			if (deep < 0) return;
			IsReversed = !IsReversed;
		}

		protected override string _getStringValue() {
			UnaryNotCondition nC = Condition as UnaryNotCondition;
			if (nC != null) {
				if (!IsReversed && !nC.IsReversed) {
					return Condition;
				}
			}

			if (IsReversed && !(Condition is RelationalCondition)) {
				return Condition;
			}

			if (!IsReversed) {
				if (Condition is RelationalCondition || Condition is BooleanCondition)
					return Condition.Reverse();
			}

			return IsReversed ? Condition : Condition.Reverse();
		}

		public override Condition Copy() {
			return new UnaryNotCondition(Condition.Copy()) { IsReversed = IsReversed, Prefix = Prefix, Suffix = Suffix };
		}

		public override void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList) {
			Condition.ToPredicate(settings, out var predicateSingleLocal, out var predicateListLocal);
			predicateSingle = new Func<ReadableTuple, string, bool>((t, s) => {
				if (predicateListLocal != null) {
					return predicateListLocal(t, s).Any(p => IsReversed ? p : !p);
				}

				return IsReversed ? predicateSingleLocal(t, s) : !predicateSingleLocal(t, s);
			});
			predicateList = null;
		}

		public override void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList) {
			ToPredicate(settings, out var predicate, out _);
			predicateSingle = (t, s) => predicate(t, s).ToString();
			predicateList = null;
		}

		public override void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			Condition.ToDouble(settings, out var predicateSingleLocal, out var predicateListLocal, out isInt);
			predicateSingle = new Func<ReadableTuple, string, double>((t, s) => IsReversed ? predicateSingleLocal(t, s) : ~(int)predicateSingleLocal(t, s));
			predicateList = new Func<ReadableTuple, string, List<double>>((t, s) => IsReversed ? predicateListLocal(t, s) : predicateListLocal(t, s).Select(p => (double)~(int)p).ToList());
			return;
		}
	}

	public static class ConditionLogic {
		private static readonly string[] _prefixes = { "while ", "if " };
		private static readonly string[] _suffixes = { " do", " then" };

		public static Condition GetCondition(Condition value) {
			return value.Copy();
		}

		public static Condition GetCondition(string value) {
			string prefix;
			string suffix;

			value = _getAffix(value, _prefixes, out prefix);
			value = _getAffix(value, _suffixes, out suffix);

			Condition condition = _getCondition(value);

			condition.Prefix = prefix;
			condition.Suffix = suffix;
			return condition;
		}

		private static Condition _getCondition(string value) {
			Condition condition;
			
			if (value == "true" || value == "false") {
				condition = new BooleanCondition(value);
			}
			else {
				string[] values = CutBrackets(value);

				if (values.Length == 1 && values[0].StartsWith("(") && values[0].Length >= 2) {
					condition = new ParenthesisCondition(values[0].Substring(1, values[0].Length - 2));
				}
				else if (values.Length == 1 && values[0].StartsWith("not(") && values[0].EndsWith(")")) {
					condition = new NotCondition(values[0].Substring(4, values[0].Length - 5));
				}
				else if (values.Length == 1 && values[0].StartsWith("~")) {
					condition = new UnaryNotCondition(values[0].Substring(1, values[0].Length - 1).Trim(' '));
				}
				else if (values.Length == 1) {
					condition = new VariableCondition(values[0]);
				}
				else if (values.Length == 3) {
					//if (values[0].StartsWith("(")) {
					//	condition = new ParenthesisCondition(values[0].Substring(1, values[0].Length - 2));
					//}
					condition = new RelationalCondition(values[0], values[1], values[2]);
				}
				else {
					// Tougher checkup...!
					// Give priority to && and ||
					for (int i = 1; i < values.Length; i++) {
						if (values[i] == "&&" || values[i] == "||") {
							condition = new RelationalCondition(string.Join(" ", values.Take(i).ToArray()), values[i], "(" + string.Join(" ", values.Skip(i + 1).ToArray()) + ")");
							return condition;
						}
					}

					// Give priority to == and ~=
					for (int i = 1; i < values.Length; i++) {
						if (values[i] == "==" || values[i] == "~=") {
							condition = new RelationalCondition(string.Join(" ", values.Take(i).ToArray()), values[i], "(" + string.Join(" ", values.Skip(i + 1).ToArray()) + ")");
							return condition;
						}
					}

					for (int i = 1; i < values.Length; i++) {
						if (values[i] == ">" || values[i] == ">=" || values[i] == "<" || values[i] == "<=") {
							condition = new RelationalCondition(string.Join(" ", values.Take(i).ToArray()), values[i], "(" + string.Join(" ", values.Skip(i + 1).ToArray()) + ")");
							return condition;
						}
					}

					condition = new VariableCondition(value);
				}
			}

			return condition;
		}

		public static string[] CutBrackets(string value) {
			return Cut(value, '(', ')');
		}

		public static bool IsConditionCharacterWithoutSpace(char c) {
			return c == '=' || c == '<' || c == '>' || c == '~' || c == '&' || c == '|' || c == '+' || c == '/' || c == '*' || c == '-' || c == '%' || c == '^' || c == '⊃' || c == '⊅';
		}

		private static string _getAffix(string value, string[] affixes, out string oAffix) {
			int indent = LineHelper.GetIndent(value);

			foreach (string affix in affixes) {
				if (value.Contains(affix)) {
					oAffix = LineHelper.GenerateIndent(indent) + affix;
					value = value.ReplaceOnce(oAffix, "");
					return value;
				}
			}

			oAffix = null;
			return value;
		}

		public static Condition SetWhileLoop(Condition condition) {
			int indent = LineHelper.GetIndent(condition.Prefix);
			condition.Prefix = LineHelper.GenerateIndent(indent) + "while ";
			condition.Suffix = " do";
			return condition;
		}

		public static Condition SetElseIf(Condition condition) {
			condition.Prefix = LineHelper.ReplaceAfterIndent(condition.Prefix, "elseif ");
			return condition;
		}

		public static string[] Cut(string value, char start, char end) {
			List<string> values = new List<string>();
			int scope = 0;

			value = value.Replace(" and ", " && ").Replace(" or ", " || ");

			int startIndex;
			int endIndex = 0;

			for (int i = 0; i < value.Length; i++) {
				if (value[i] == start) {
					scope = 1;
					i++;

					while (scope > 0 && i < value.Length) {
						if (value[i] == start)
							scope++;
						if (value[i] == end)
							scope--;
						i++;
					}

					if (i >= value.Length - 1) {
						values.Add(value.Substring(endIndex, value.Length - endIndex));
					}
				}
				else if (value[i] == '~' && i < value.Length - 1 && value[i + 1] != '=') {
				}
				else if (IsConditionCharacterWithoutSpace(value[i])) {
					startIndex = i;

					if (startIndex > endIndex) {
						values.Add(value.Substring(endIndex, startIndex - endIndex).Trim(' '));
					}

					while (IsConditionCharacterWithoutSpace(value[i])) {
						i++;

						if (value[i] == '~')
							break;
					}

					values.Add(value.Substring(startIndex, i - startIndex));
					endIndex = i;
				}
				else if (i == value.Length - 1) {
					values.Add(value.Substring(endIndex, value.Length - endIndex));
				}
			}

			return values.Select(p => p.Trim(' ')).ToArray();
		}
	}

	public abstract class Condition {
		public string Prefix { get; set; }
		public string Suffix { get; set; }

		public Condition Reverse() {
			Reverse(1);
			return this;
		}

		internal abstract void Reverse(int deep);
		protected abstract string _getStringValue();

		public override string ToString() {
			return Prefix + _getStringValue() + Suffix;
		}

		public static implicit operator string(Condition condition) {
			return condition.ToString();
		}

		public abstract Condition Copy();

		public abstract void ToPredicate(TabSettings settings, out Func<ReadableTuple, string, bool> predicateSingle, out Func<ReadableTuple, string, List<bool>> predicateList);

		public virtual void ToDouble(TabSettings settings, out Func<ReadableTuple, string, double> predicateSingle, out Func<ReadableTuple, string, List<double>> predicateList, out bool isInt) {
			isInt = false;
			predicateSingle = new Func<ReadableTuple, string, double>((t, s) => 0);
			predicateList = null;
		}

		public abstract void ToValue(TabSettings settings, out Func<ReadableTuple, string, string> predicateSingle, out Func<ReadableTuple, string, List<string>> predicateList);
	}

	public enum PrintMode {
		WithAffixes,
		WithoutAffixes
	}

	public enum RelationalComparison {
		None,
		Le,
		Lt,
		Contains,
		Exclude,
		Eq,
		NotEq,
		Ge,
		Gt,
		And,
		Or,
		BinaryAnd,
		BinaryOr,
		BinaryLeftShift,
		BinaryRightShift,
		Add,
		Minus,
		Mult,
		Div,
		Mod,
		Pow,
		Not,
	}
}