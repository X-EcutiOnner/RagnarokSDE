using ErrorManager;
using Lua.Function;
using SDE.View.Editors.ScriptEdit.Athena;
using System;
using System.Text;

namespace SDE.View.Editors.ScriptEdit {
	public class AthenaAstWriter {
		public unsafe static string ToScript(string equipmentLuaScript) {
			// Convert the input lua script string into a byte array for the lua lexer.
			var data = Encoding.Default.GetBytes(equipmentLuaScript.ToString());

			fixed (byte* pData = data) {
				var luaLexer = new Lexer(pData, data.Length);

				try {
					// Convert the script tokens into an lua AST.
					var luaAstParser = new LuaAstParser(luaLexer, Encoding.Default);

					// The AST will give a chunk of assignments
					var luaChunk = luaAstParser.Parse();

					// Prepare to convert the lua AST to athena AST
					var converter = new LuaAstToAthenaAst();
					AthenaBlock athenaAst;

					// The equipment lua script string will most likely be the entire local function, for example:
					// function()
					//	AddAttrTolerace(5, 20)
					// end
					// We don't want to parse the "function()" part of the script, but its content, so the code below parses the function content only.
					if (luaChunk.Statements.Count == 1 && 
						luaChunk.Statements[0] is LuaExpressionStatement expression &&
						expression.Expression is LuaFunctionExpression functionExpression) {
						athenaAst = converter.Convert(functionExpression.Body);
					}
					else {
						athenaAst = converter.Convert(luaChunk);
					}

					StringBuilder b = new StringBuilder();
					WriteBlock(b, athenaAst);
					return b.ToString();
				}
				catch {
					return null;
				}
			}
		}

		private static void WriteBlock(StringBuilder b, AthenaBlock block) {
			foreach (var statement in block.Statements) {
				WriteStatement(b, statement);
			}
		}

		private static void WriteStatement(StringBuilder b, AthenaStatement statement) {
			switch (statement) {
				case AthenaAssignmentStatement assignment:
					WriteExpression(b, assignment.Target);
					b.Append(" = ");
					WriteExpression(b, assignment.Value);
					b.Append("; ");
					break;
				case AthenaIfStatement ifStatement:
					WriteIfStatement(b, ifStatement);
					break;
				case AthenaExpressionStatement expression:
					if (expression.Expression is NullExpression)
						return;

					WriteExpression(b, expression.Expression);
					b.Append(";");
					break;
				case AthenaBlockStatement blockStatement:
					WriteBlockStatement(b, blockStatement);
					break;
				default:
					throw new Exception($"Unrecognized statement type: {statement.GetType()}.");
			}
		}

		private static void WriteBlockStatement(StringBuilder b, AthenaBlockStatement blockStatement) {
			foreach (var statement in blockStatement.Statements)
				WriteStatement(b, statement);
		}

		private static void WriteIfStatement(StringBuilder b, AthenaIfStatement ifStatement) {
			for (int i = 0; i < ifStatement.Branches.Count; i++) {
				if (i == 0)
					b.Append("if (");
				else
					b.Append("else if (");

				var branch = ifStatement.Branches[i];

				WriteExpression(b, branch.Condition);
				b.Append(") { ");
				WriteBlock(b, branch.Body);
				b.Append("} ");
			}

			if (ifStatement.Else != null) {
				b.Append("else { ");
				WriteBlock(b, ifStatement.Else);
				b.Append("} ");
			}
		}

		private static void WriteExpression(StringBuilder b, AthenaExpression expression, int parentPrecedence = 0) {
			switch (expression) {
				case AthenaVariableExpression variable:
					b.Append(variable.Name);
					break;
				case AthenaLiteralExpression literal:
					WriteLiteral(b, literal);
					break;
				case AthenaBinaryExpression binary:
					WriteBinaryExpression(b, binary, parentPrecedence);
					break;
				case AthenaUnaryExpression unary:
					WriteUnaryExpression(b, unary, parentPrecedence);
					break;
				case AthenaCallExpression call:
					WriteCall(b, call);
					break;
				case NullExpression _:
					break;
				default:
					throw new Exception($"Unrecognized expression type: {expression.GetType()}.");
			}
		}

		private static void WriteUnaryExpression(StringBuilder b, AthenaUnaryExpression unary, int parentPrecedence) {
			switch (unary.Operator) {
				case LuaUnaryOperator.Negate:
					b.Append('-');
					WriteExpression(b, unary.Operand, 11);
					break;
				default:
					throw new Exception($"Unrecognized unary type and operator: {unary.GetType()}, {unary.Operator}.");
			}
		}

		private static void WriteBinaryExpression(StringBuilder b, AthenaBinaryExpression binary, int parentPrecedence) {
			switch (binary.Operator) {
				case LuaBinaryOperator.Power:
					b.Append("pow(");
					WriteExpression(b, binary.Left);
					b.Append(", ");
					WriteExpression(b, binary.Right);
					b.Append(")");
					return;
			}

			int precedence = GetPrecedence(binary.Operator);

			bool parentheses = precedence < parentPrecedence;

			if (parentheses)
				b.Append('(');

			WriteExpression(b, binary.Left, precedence);
			WriteOperator(b, binary.Operator);
			WriteExpression(b, binary.Right, precedence);

			if (parentheses)
				b.Append(')');
		}

		private static void WriteLiteral(StringBuilder b, AthenaLiteralExpression literal) {
			switch (literal.Type) {
				case AthenaLiteralType.String:
					b.Append("\"" + literal.StringValue + "\"");
					break;
				case AthenaLiteralType.Boolean:
					b.Append(literal.BooleanValue ? "1" : "0");
					break;
				case AthenaLiteralType.Number:
					b.Append((int)literal.NumberValue);
					break;
				case AthenaLiteralType.Constant:
					b.Append(literal.StringValue);
					break;
				default:
					throw new Exception($"Unrecognized literal expression type: {literal.GetType()}.");
			}
		}

		private static void WriteCall(StringBuilder b, AthenaCallExpression call) {
			b.Append(call.Name);

			bool parenthesis = true;
			
			switch (call.Name) {
				case "skill":
				case "bonus":
				case "bonus2":
				case "bonus3":
				case "bonus4":
				case "bonus5":
					parenthesis = false;
					break;
			}

			if (parenthesis)
				b.Append("(");
			else
				b.Append(" ");

			for (int i = 0; i < call.Arguments.Count; i++) {
				WriteExpression(b, call.Arguments[i]);

				if (i < call.Arguments.Count - 1)
					b.Append(",");
			}

			if (parenthesis)
				b.Append(")");
		}

		private static void WriteOperator(StringBuilder b, LuaBinaryOperator @operator) {
			switch (@operator) {
				case LuaBinaryOperator.Add:				b.Append("+"); break;
				case LuaBinaryOperator.Subtract:		b.Append("-"); break;
				case LuaBinaryOperator.Multiply:		b.Append("*"); break;
				case LuaBinaryOperator.Divide:			b.Append("/"); break;
				case LuaBinaryOperator.FloorDivide:		b.Append("/"); break;
				//case LuaBinaryOperator.Modulo: b.Append(" % "); break;
				//case LuaBinaryOperator.Power: b.Append("pow("); break;
				case LuaBinaryOperator.Concat:			b.Append("+"); break;
				case LuaBinaryOperator.Equal:			b.Append(" == "); break;
				case LuaBinaryOperator.NotEqual:		b.Append(" != "); break;
				case LuaBinaryOperator.Less:			b.Append("<"); break;
				case LuaBinaryOperator.LessEqual:		b.Append("<="); break;
				case LuaBinaryOperator.Greater:			b.Append(">"); break;
				case LuaBinaryOperator.GreaterEqual:	b.Append(">="); break;
				case LuaBinaryOperator.And:				b.Append(" && "); break;
				case LuaBinaryOperator.Or:				b.Append(" || "); break;
				case LuaBinaryOperator.BitwiseAnd:		b.Append("&"); break;
				case LuaBinaryOperator.BitwiseOr:		b.Append("|"); break;
				case LuaBinaryOperator.BitwiseXor:		b.Append("^"); break;
				case LuaBinaryOperator.ShiftLeft:		b.Append("<<"); break;
				case LuaBinaryOperator.ShiftRight:		b.Append(">>"); break;
				default:
					throw new Exception($"Unrecognized operator: {@operator}.");
			}
		}

		private static int GetPrecedence(LuaBinaryOperator op) {
			switch (op) {
				case LuaBinaryOperator.Or:
					return 1;
				case LuaBinaryOperator.And:
					return 2;
				case LuaBinaryOperator.BitwiseOr:
					return 3;
				case LuaBinaryOperator.BitwiseXor:
					return 4;
				case LuaBinaryOperator.BitwiseAnd:
					return 5;
				case LuaBinaryOperator.Equal:
				case LuaBinaryOperator.NotEqual:
				case LuaBinaryOperator.Less:
				case LuaBinaryOperator.LessEqual:
				case LuaBinaryOperator.Greater:
				case LuaBinaryOperator.GreaterEqual:
					return 6;
				case LuaBinaryOperator.ShiftLeft:
				case LuaBinaryOperator.ShiftRight:
					return 7;
				case LuaBinaryOperator.Concat:
					return 8;
				case LuaBinaryOperator.Add:
				case LuaBinaryOperator.Subtract:
					return 9;
				case LuaBinaryOperator.Multiply:
				case LuaBinaryOperator.Divide:
				case LuaBinaryOperator.FloorDivide:
				case LuaBinaryOperator.Modulo:
					return 10;
				case LuaBinaryOperator.Power:
					return 11;
				default:
					throw new ArgumentOutOfRangeException(nameof(op));
			}
		}

		private static int GetPrecedence(AthenaExpression expression) {
			switch (expression) {
				case AthenaBinaryExpression binary:
					return GetPrecedence(binary.Operator);
				case AthenaUnaryExpression unary:
					return 11;
				default:
					return int.MaxValue;
			}
		}
	}
}
