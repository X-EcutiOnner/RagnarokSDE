using Lua.Function;
using System.Collections.Generic;

namespace SDE.View.Editors.ScriptEdit {
	public abstract class AthenaNode {
	}

	public abstract class AthenaStatement : AthenaNode {
	}

	public abstract class AthenaExpression : AthenaNode {
	}

	public sealed class AthenaBlock : AthenaNode {
		public List<AthenaStatement> Statements { get; } = new List<AthenaStatement>();
	}

	public sealed class AthenaVariableExpression : AthenaExpression {
		public string Name { get; set; }

		public AthenaVariableExpression(string name) {
			Name = name;
		}
	}

	public enum AthenaLiteralType {
		Number,
		String,
		Boolean,
		Constant
	}

	public sealed class AthenaLiteralExpression : AthenaExpression {
		public AthenaLiteralType Type { get; }

		public double NumberValue { get; set; }
		public string StringValue { get; set; }
		public bool BooleanValue { get; set; }

		public AthenaLiteralExpression(double value) {
			Type = AthenaLiteralType.Number;
			NumberValue = value;
		}

		public AthenaLiteralExpression(string value) {
			Type = AthenaLiteralType.String;
			StringValue = value;
		}

		public AthenaLiteralExpression(bool value) {
			Type = AthenaLiteralType.Boolean;
			BooleanValue = value;
		}

		public AthenaLiteralExpression(string value, bool isConstant = false) {
			if (isConstant)
				Type = AthenaLiteralType.Constant;
			else
				Type = AthenaLiteralType.String;

			StringValue = value;
		}
	}

	public sealed class AthenaBinaryExpression : AthenaExpression {
		public AthenaExpression Left { get; set; }
		public LuaBinaryOperator Operator { get; set; }
		public AthenaExpression Right { get; set; }

		public AthenaBinaryExpression(AthenaExpression left, LuaBinaryOperator @operator, AthenaExpression right) {
			Left = left;
			Operator = @operator;
			Right = right;
		}
	}

	public sealed class AthenaCallExpression : AthenaExpression {
		public string Name { get; set; }

		public List<AthenaExpression> Arguments { get; } = new List<AthenaExpression>();

		public AthenaCallExpression() {
		}

		public AthenaCallExpression(string name) {
			Name = name;
		}
	}

	public sealed class NullExpression : AthenaExpression {
	}

	public sealed class AthenaAssignmentStatement : AthenaStatement {
		public AthenaExpression Target { get; set; }
		public AthenaExpression Value { get; set; }
	}

	public sealed class AthenaIfBranch : AthenaStatement {
		public AthenaExpression Condition { get; set; }
		public AthenaBlock Body { get; set; }
	}

	public sealed class AthenaIfStatement : AthenaStatement {
		public List<AthenaIfBranch> Branches { get; } = new List<AthenaIfBranch>();
		public AthenaBlock Else { get; set; }
	}

	public sealed class AthenaExpressionStatement : AthenaStatement {
		public AthenaExpression Expression { get; set; }

		public AthenaExpressionStatement(AthenaExpression expression) {
			Expression = expression;
		}
	}

	public sealed class AthenaBlockStatement : AthenaStatement {
		public List<AthenaStatement> Statements { get; } = new List<AthenaStatement>();

		public AthenaBlockStatement() {
		}
	}

	public sealed class AthenaUnaryExpression : AthenaExpression {
		public LuaUnaryOperator Operator { get; set; }
		public AthenaExpression Operand { get; set; }

		public AthenaUnaryExpression() {
		}

		public AthenaUnaryExpression(LuaUnaryOperator @operator, AthenaExpression operand) {
			Operator = @operator;
			Operand = operand;
		}
	}
}
