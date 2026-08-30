using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SDE.Core {
	public static class StructuralComparer<T> {
		private static readonly Func<T, T, bool> _equals = BuildEquals();
		private static readonly Func<T, int> _hashCode = BuildHashCode();

		public static bool Equals(T x, T y) => _equals(x, y);
		public static int GetHashCode(T value) => _hashCode(value);

		private static Func<T, T, bool> BuildEquals() {
			var x = Expression.Parameter(typeof(T), "x");
			var y = Expression.Parameter(typeof(T), "y");
			var body = BuildEqualsExpression(typeof(T), x, y);

			return Expression.Lambda<Func<T, T, bool>>(body, x, y).Compile();
		}

		private static Func<T, int> BuildHashCode() {
			var value = Expression.Parameter(typeof(T), "value");
			var body = BuildHashCodeExpression(typeof(T), value);

			return Expression.Lambda<Func<T, int>>(body, value).Compile();
		}

		private static Expression BuildEqualsExpression(Type type, Expression x, Expression y) {
			// Reference-type null handling.
			if (!type.IsValueType) {
				var bothNull = Expression.AndAlso(
					Expression.Equal(x, Expression.Constant(null, type)),
					Expression.Equal(y, Expression.Constant(null, type)));

				var oneNull = Expression.OrElse(
					Expression.Equal(x, Expression.Constant(null, type)),
					Expression.Equal(y, Expression.Constant(null, type)));

				var content = BuildEqualsNonNullExpression(type, x, y);

				return Expression.OrElse(
					bothNull,
					Expression.AndAlso(
						Expression.Not(oneNull),
						content));
			}

			return BuildEqualsNonNullExpression(type, x, y);
		}

		private static Expression BuildEqualsNonNullExpression(Type type, Expression x, Expression y) {
			// Lists are compared element-by-element.
			if (TryGetListElementType(type, out var elementType))
				return BuildListEquals(type, elementType, x, y);

			// Primitive/value framework types, strings, enums, etc.
			if (IsLeafType(type))
				return BuildEqualityComparerEquals(type, x, y);

			// Structs/classes containing fields.
			var fields = GetInstanceFields(type);

			if (fields.Length == 0)
				return BuildEqualityComparerEquals(type, x, y);

			Expression result = Expression.Constant(true);

			foreach (var field in fields) {
				var left = Expression.Field(x, field);
				var right = Expression.Field(y, field);
				var fieldEquals = BuildEqualsExpression( field.FieldType, left, right);

				result = Expression.AndAlso(result, fieldEquals);
			}

			return result;
		}

		private static Expression BuildListEquals(Type listType, Type elementType, Expression x, Expression y) {
			var countX = Expression.Property(x, "Count");
			var countY = Expression.Property(y, "Count");
			var countEqual = Expression.Equal(countX, countY);
			var index = Expression.Variable(typeof(int), "i");
			var itemX = Expression.Property(x, "Item", index);
			var itemY = Expression.Property(y, "Item", index);
			var itemEquals = BuildEqualsExpression(elementType, itemX, itemY);

			var loop = Expression.Loop(
				Expression.IfThenElse(
					Expression.LessThan(
						index,
						countX),

					Expression.Block(
						Expression.IfThen(
							Expression.Not(itemEquals),
							Expression.Break(
								Expression.Label())),
						Expression.PostIncrementAssign(index)),

					Expression.Break(
						Expression.Label())));

			// A simpler approach is preferable here: use Enumerable.SequenceEqual
			// with a generated comparer for the element type.
			return BuildListEqualsViaEnumerable( elementType, x, y);
		}

		private static Expression BuildListEqualsViaEnumerable(Type elementType, Expression x, Expression y) {
			var comparerType = typeof(StructuralEqualityComparer<>).MakeGenericType(elementType);

			var comparer = Expression.New(comparerType);

			var sequenceEqual = typeof(Enumerable)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(m =>
					m.Name == nameof(Enumerable.SequenceEqual) &&
					m.GetParameters().Length == 3 &&
					m.GetParameters()[2].ParameterType.IsGenericType &&
					m.GetParameters()[2].ParameterType.GetGenericTypeDefinition()
						== typeof(IEqualityComparer<>));

			var method = sequenceEqual.MakeGenericMethod(elementType);

			return Expression.Call(method, x, y, comparer);
		}

		private static Expression BuildEqualityComparerEquals(Type type, Expression x, Expression y) {
			var comparerType = typeof(EqualityComparer<>).MakeGenericType(type);
			var defaultProperty = comparerType.GetProperty("Default");
			var equalsMethod = comparerType.GetMethod("Equals", new[] { type, type });
			var comparer = Expression.Property(null, defaultProperty);

			return Expression.Call(comparer, equalsMethod, x, y);
		}

		private static Expression BuildHashCodeExpression(Type type, Expression value) {
			// Null reference.
			if (!type.IsValueType) {
				var nullCheck = Expression.Equal(value, Expression.Constant(null, type));

				var nonNullHash = BuildHashCodeNonNullExpression(type, value);

				return Expression.Condition(
					nullCheck,
					Expression.Constant(0),
					nonNullHash);
			}

			return BuildHashCodeNonNullExpression(type, value);
		}

		private static Expression BuildHashCodeNonNullExpression(
			Type type,
			Expression value) {
			if (TryGetListElementType(type, out var elementType))
				return BuildListHashCode(elementType, value);

			if (IsLeafType(type))
				return BuildEqualityComparerHashCode(type, value);

			var fields = GetInstanceFields(type);

			if (fields.Length == 0)
				return BuildEqualityComparerHashCode(type, value);

			// hash = 17;
			Expression hash = Expression.Constant(17);

			foreach (var field in fields) {
				var fieldValue = Expression.Field(value, field);

				var fieldHash = BuildHashCodeExpression(field.FieldType, fieldValue);

				// hash = hash * 31 + fieldHash
				hash = Expression.Add(
					Expression.Multiply(
						hash,
						Expression.Constant(31)),
					fieldHash);
			}

			return hash;
		}

		private static Expression BuildListHashCode(Type elementType, Expression list) {
			var helperType = typeof(StructuralHashCodeHelper<>).MakeGenericType(elementType);
			var method = helperType.GetMethod(nameof(StructuralHashCodeHelper<int>.Calculate));
			return Expression.Call(method, list);
		}

		private static Expression BuildEqualityComparerHashCode(Type type, Expression value) {
			var comparerType = typeof(EqualityComparer<>).MakeGenericType(type);
			var defaultProperty = comparerType.GetProperty("Default");
			var hashMethod = comparerType.GetMethod("GetHashCode", new[] { type });
			var comparer = Expression.Property(null, defaultProperty);

			return Expression.Call(comparer, hashMethod, value);
		}

		private static bool TryGetListElementType(Type type, out Type elementType) {
			if (type.IsGenericType &&
				type.GetGenericTypeDefinition() == typeof(List<>)) {
				elementType = type.GetGenericArguments()[0];
				return true;
			}

			elementType = null;
			return false;
		}

		private static bool IsLeafType(Type type) {
			if (type.IsPrimitive ||
				type.IsEnum ||
				type == typeof(string) ||
				type == typeof(decimal) ||
				type == typeof(DateTime) ||
				type == typeof(DateTimeOffset) ||
				type == typeof(TimeSpan) ||
				type == typeof(Guid)) {
				return true;
			}

			// Other framework structs/classes should use their normal equality.
			if (type.Namespace != null &&
				type.Namespace.StartsWith("System.", StringComparison.Ordinal)) {
				return true;
			}

			return false;
		}

		private static FieldInfo[] GetInstanceFields(Type type) {
			return type
				.GetFields(
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic)
				.Where(f => !f.IsStatic)
				.ToArray();
		}
	}

	/// <summary>
	/// EqualityComparer adapter which redirects equality to
	/// StructuralComparer<T>.
	/// </summary>
	internal sealed class StructuralEqualityComparer<T> : IEqualityComparer<T> {
		public bool Equals(T x, T y) {
			return StructuralComparer<T>.Equals(x, y);
		}

		public int GetHashCode(T obj) {
			return StructuralComparer<T>.GetHashCode(obj);
		}
	}

	/// <summary>
	/// Helper used by the generated hash-code expression for Lists.
	/// </summary>
	internal static class StructuralHashCodeHelper<T> {
		public static int Calculate(List<T> list) {
			if (list == null)
				return 0;

			unchecked {
				int hash = 17;

				foreach (var item in list) {
					hash = hash * 31 +
						StructuralComparer<T>.GetHashCode(item);
				}

				return hash;
			}
		}
	}
}