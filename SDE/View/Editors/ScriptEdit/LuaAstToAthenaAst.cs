using Lua.Function;
using SDE.Databases;
using SDE.Databases.Generic.Common;
using SDE.Databases.Generic.Parser;
using SDE.Databases.Mobs.Common;
using SDE.Databases.Skills.Common;
using SDE.Databases.Skills.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDE.View.Editors.ScriptEdit.Athena {
	public sealed class LuaAstToAthenaAst {
		public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

		public AthenaBlock Convert(LuaChunk chunk) {
			AthenaBlock result = new AthenaBlock();

			foreach (LuaStatement statement in chunk.Statements)
				result.Statements.Add(ConvertStatement(statement));

			return result;
		}

		public AthenaBlock Convert(LuaBlock block) {
			AthenaBlock result = new AthenaBlock();

			foreach (LuaStatement statement in block.Statements)
				result.Statements.Add(ConvertStatement(statement));

			return result;
		}

		private AthenaStatement ConvertStatement(LuaStatement statement) {
			switch (statement) {
				case LuaLocalDeclarationStatement local:
					return ConvertLocal(local);
				case LuaAssignmentStatement assignment:
					return ConvertAssignment(assignment);
				case LuaExpressionStatement expression:
					return ConvertExpressionStatement(expression);
				case LuaIfStatement @if:
					return ConvertIf(@if);
				default:
					throw new NotSupportedException($"Unexpected Lua statement type {statement.GetType().Name}.");
			}
		}

		private AthenaStatement ConvertLocal(LuaLocalDeclarationStatement statement) {
			AthenaBlockStatement result = new AthenaBlockStatement();

			for (int i = 0; i < statement.Names.Count; i++) {
				AthenaAssignmentStatement assignment = new AthenaAssignmentStatement();

				var varName = ".@" + statement.Names[i];
				assignment.Target = new AthenaVariableExpression(varName);

				if (i < statement.Values.Count) {
					if (statement.Values[i] is LuaLiteralExpression lit && lit.Type == LuaLiteralType.Number && (double)lit.Value == 0)
						continue;

					assignment.Value = ConvertExpression(statement.Values[i]);

					// Add a dummy entry to prevent renaming a variable that was not set to 0
					Variables[varName] = varName;
				}
				else {
					continue;
				}

				result.Statements.Add(assignment);
			}

			return result;
		}

		private AthenaStatement ConvertAssignment(LuaAssignmentStatement statement) {
			if (statement.Targets.Count != 1 || statement.Values.Count != 1) {
				throw new NotSupportedException("Multiple assignment is not supported.");
			}

			var result = new AthenaAssignmentStatement {
				Target = ConvertAssignmentTarget(statement.Targets[0]),
				Value = ConvertExpression(statement.Values[0])
			};

			if (result.Target is AthenaVariableExpression variable && !Variables.ContainsKey(variable.Name)) {
				if (TryRename(result, variable, ".@r", "getrefine") ||
					TryRename(result, variable, ".@r2", "getrefine") ||
					TryRename(result, variable, ".@g", "getenchantgrade") ||
					TryRename(result, variable, ".@wlv", "getequipweaponlv") ||
					TryRename(result, variable, ".@alv", "getequiparmorlv") ||
					TryRename(result, variable, ".@i", "getpetinfo") ||
					TryRename(result, variable, ".@type", "getiteminfo") ||
					TryRename2(result, variable, ".@blv", "BaseLevel") ||
					TryRename2(result, variable, ".@jlv", "JobLevel")) {
				}
			}

			return result;
		}

		private bool TryRename(AthenaAssignmentStatement assignment, AthenaVariableExpression variable, string nName, string methodName) {
			if (assignment.Value is AthenaCallExpression call && call.Name == methodName && !Variables.Values.Any(p => p == nName)) {
				Variables[variable.Name] = nName;
				variable.Name = nName;
				return true;
			}

			return false;
		}

		private bool TryRename2(AthenaAssignmentStatement assignment, AthenaVariableExpression variable, string nName, string methodName) {
			if (assignment.Value is AthenaLiteralExpression literal && literal.Type == AthenaLiteralType.Constant && literal.StringValue == methodName && !Variables.Values.Any(p => p == nName)) {
				Variables[variable.Name] = nName;
				variable.Name = nName;
				return true;
			}

			return false;
		}

		private AthenaExpression ConvertAssignmentTarget(LuaExpression expression) {
			if (expression is LuaIdentifierExpression identifier) {
				return new AthenaVariableExpression(RenameVariableName(".@" + identifier.Name));
			}

			throw new NotSupportedException("Unsupported assignment target.");
		}

		private string RenameVariableName(string name) {
			if (Variables.TryGetValue(name, out string nName)) {
				return nName;
			}

			return name;
		}

		private AthenaStatement ConvertIf(LuaIfStatement statement) {
			AthenaIfStatement result = new AthenaIfStatement();

			foreach (var branch in statement.Branches) {
				result.Branches.Add(ConvertIfBranch(branch));
			}

			if (statement.Else != null) {
				result.Else = ConvertBlock(statement.Else);
			}

			return result;
		}

		private AthenaIfBranch ConvertIfBranch(LuaIfBranch branch) {
			return new AthenaIfBranch {
				Condition = ConvertExpression(branch.Condition),
				Body = ConvertBlock(branch.Body),
			};
		}

		private AthenaBlock ConvertBlock(LuaBlock block) {
			AthenaBlock result = new AthenaBlock();

			foreach (LuaStatement statement in block.Statements)
				result.Statements.Add(ConvertStatement(statement));

			return result;
		}

		private AthenaExpression ConvertExpression(LuaExpression expression) {
			switch (expression) {
				case LuaLiteralExpression literal:
					return ConvertLiteral(literal);
				case LuaIdentifierExpression identifier:
					return new AthenaVariableExpression(RenameVariableName(".@" + identifier.Name));
				case LuaBinaryExpression binary:
					return new AthenaBinaryExpression(ConvertExpression(binary.Left), binary.Operator, ConvertExpression(binary.Right));
				case LuaCallExpression call:
					return ConvertCall(call);
				default:
					throw new NotSupportedException($"Unexpected Lua expression {expression.GetType().Name}.");
			}
		}

		private AthenaExpression ConvertLiteral(LuaLiteralExpression literal) {
			switch (literal.Type) {
				case LuaLiteralType.String:
					return new AthenaLiteralExpression((string)literal.Value);
				case LuaLiteralType.Boolean:
					return new AthenaLiteralExpression((bool)literal.Value);
				case LuaLiteralType.Number:
					return new AthenaLiteralExpression((int)(double)literal.Value);
				case LuaLiteralType.Nil:
					return new AthenaLiteralExpression(0);
				default:
					throw new InvalidOperationException($"Unknown literal type {literal.Type}.");
			}
		}

		private void ArgumentCheck(LuaCallExpression call, int count) {
			var name = ((LuaIdentifierExpression)call.Target).Name;

			if (call.Arguments.Count != count)
				throw new InvalidOperationException($"Method {name} expects {count} arguments, but found {call.Arguments.Count}.");
		}

		private AthenaExpression ConvertCall(LuaCallExpression call) {
			if (call.Target is LuaMemberExpression member) {
				if (member.Object is LuaIdentifierExpression ident && ident.Name == "math" && member.Member == "floor") {
					return ConvertCallToFunction(call, "");
				}
			}

			if (!(call.Target is LuaIdentifierExpression identifier)) {
				throw new NotSupportedException("Only simple function calls are supported.");
			}

			// Attribute = Element
			// 

			switch (identifier.Name) {
				case "AddExtParam": return ConvertAddExtParam(call);
				case "SubExtParam": return ConvertAddExtParam(call, ArgumentType.NegateInt);

				case "AddDamage_Size":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bSubSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.NegateInt);
					else
						return ConvertCallToBonus(call, "bAddSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.Int);
				case "SubDamage_Size":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bSubSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.Int);
					else
						return ConvertCallToBonus(call, "bAddSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.NegateInt);

				// On self, this is applied through AddDamage_Size / bSubSize 99% of the time, let's skip it
				case "AddMDamage_Size":
					if (IsUnit(call, 0, Unit.User))
						//return ConvertCallToBonus(call, "bMagicSubSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.NegateInt);
						return new NullExpression();
					else
						return ConvertCallToBonus(call, "bMagicAddSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.Int);
				case "SubMDamage_Size":
					if (IsUnit(call, 0, Unit.User))
						//return ConvertCallToBonus(call, "bMagicSubSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.Int);
						return new NullExpression();
					else
						return ConvertCallToBonus(call, "bMagicAddSize", ArgumentType.Skip, ArgumentType.Size, ArgumentType.NegateInt);

				case "ClassAddDamage":
					if (IsUnit(call, 1, Unit.User))
						return ConvertCallToBonus(call, "bSubClass", ArgumentType.Class, ArgumentType.Skip, ArgumentType.NegateInt);
					else
						return ConvertCallToBonus(call, "bAddClass", ArgumentType.Class, ArgumentType.Skip, ArgumentType.Int);
				case "ClassSubDamage":
					if (IsUnit(call, 1, Unit.User))
						return ConvertCallToBonus(call, "bSubClass", ArgumentType.Class, ArgumentType.Skip, ArgumentType.Int);
					else
						return ConvertCallToBonus(call, "bAddClass", ArgumentType.Class, ArgumentType.Skip, ArgumentType.NegateInt);

				// Add damage to enemies of [Element] by X%
				// Reduce damage taken from enemies of [Element] by X%
				case "AddDamage_Property":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bSubDefEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.NegateInt);
					else
						return ConvertCallToBonus(call, "bAddEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.Int);
				case "SubDamage_Property":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bSubDefEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.Int);
					else
						return ConvertCallToBonus(call, "bAddEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.NegateInt);

				// Add magic damage to enemies of [Element] by X%
				// Reduce magic damage taken from enemies of [Element] by X%
				case "AddMDamage_Property":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bMagicSubDefEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.NegateInt);
					else
						return ConvertCallToBonus(call, "bMagicAddEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.Int);
				case "SubMDamage_Property":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bMagicSubDefEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.Int);
					else
						return ConvertCallToBonus(call, "bMagicAddEle", ArgumentType.Skip, ArgumentType.Element, ArgumentType.NegateInt);

				case "AddMeleeAttackDamage":
					if (IsUnit(call, 0, Unit.User)) 
						return ConvertCallToBonus(call, "bNearAtkDef", ArgumentType.Skip, ArgumentType.NegateInt);
					else
						return ConvertCallToBonus(call, "bShortAtkRate", ArgumentType.Skip, ArgumentType.Int);
				case "SubMeleeAttackDamage":
					if (IsUnit(call, 0, Unit.User))
						return ConvertCallToBonus(call, "bNearAtkDef", ArgumentType.Skip, ArgumentType.Int);
					else
						return ConvertCallToBonus(call, "bShortAtkRate", ArgumentType.Skip, ArgumentType.NegateInt);

				case "AddRangeAttackDamage": return ConvertCallToBonus(call, "bLongAtkRate", ArgumentType.Skip, ArgumentType.Int);
				case "SubRangeAttackDamage": return ConvertCallToBonus(call, "bLongAtkRate", ArgumentType.Skip, ArgumentType.NegateInt);

				case "AddRaceTolerace":	return ConvertCallToBonus(call, "bSubRace", ArgumentType.Race, ArgumentType.Int);
				case "SubRaceTolerace":	return ConvertCallToBonus(call, "bSubRace", ArgumentType.Race, ArgumentType.NegateInt);

				// Add/Reduce damage taken from [Element] by X%
				case "AddAttrTolerace":	return ConvertCallToBonus(call, "bSubEle", ArgumentType.Element, ArgumentType.Int);
				case "SubAttrTolerace":	return ConvertCallToBonus(call, "bSubEle", ArgumentType.Element, ArgumentType.NegateInt);

				case "addattrtolerace":	return ConvertCallToBonus(call, "bSubEle", ArgumentType.Element, ArgumentType.Int, ArgumentType.BattleFlag);
				case "subattrtolerace":	return ConvertCallToBonus(call, "bSubEle", ArgumentType.Element, ArgumentType.NegateInt, ArgumentType.BattleFlag);

				case "AddSpellDelay":	return ConvertCallToBonus(call, "bDelayrate", ArgumentType.Int);
				case "SubSpellDelay":	return ConvertCallToBonus(call, "bDelayrate", ArgumentType.NegateInt);

				case "AddSpellCastTime":	return ConvertCallToBonus(call, "bVariableCastrate", ArgumentType.Int);
				case "SubSpellCastTime":	return ConvertCallToBonus(call, "bVariableCastrate", ArgumentType.NegateInt);

				case "RaceAddDamage":		return ConvertCallToBonus(call, "bAddRace", ArgumentType.Race, ArgumentType.Int);
				case "RaceSubDamage":		return ConvertCallToBonus(call, "bAddRace", ArgumentType.Race, ArgumentType.NegateInt);

				case "RaceAddDamageSelf":	return ConvertCallToBonus(call, "bSubRace", ArgumentType.Race, ArgumentType.NegateInt);
				case "RaceSubDamageSelf":	return ConvertCallToBonus(call, "bSubRace", ArgumentType.Race, ArgumentType.Int);

				case "AddHealModifyPercent": return ConvertCallToBonus(call, "bAddItemHealRate", ArgumentType.Int);
				case "SubHealModifyPercent": return ConvertCallToBonus(call, "bAddItemHealRate", ArgumentType.NegateInt);

				case "AddSkillSP": return ConvertCallToBonus(call, "bSkillUseSP", ArgumentType.Skill, ArgumentType.NegateInt);
				case "SubSkillSP": return ConvertCallToBonus(call, "bSkillUseSP", ArgumentType.Skill, ArgumentType.Int);

				case "AddSPconsumption": return ConvertCallToBonus(call, "bUseSPrate", ArgumentType.Int);
				case "SubSPconsumption": return ConvertCallToBonus(call, "bUseSPrate", ArgumentType.NegateInt);

				case "addspconsumption": return ConvertSkillSpConsumption(call);
				case "subspconsumption": return ConvertSkillSpConsumption(call, negate: true);

				case "AddMdamage_Race": return ConvertCallToBonus(call, "bMagicAddRace", ArgumentType.Race, ArgumentType.Int);
				case "SubMdamage_Race": return ConvertCallToBonus(call, "bMagicAddRace", ArgumentType.Race, ArgumentType.NegateInt);

				case "AddSFCTEquipAmount": return ConvertCallToBonus(call, "bFixedCast", ArgumentType.Skip, ArgumentType.Int, ArgumentType.Skip);
				case "SubSFCTEquipAmount": return ConvertCallToBonus(call, "bFixedCast", ArgumentType.Skip, ArgumentType.NegateInt, ArgumentType.Skip);

				case "AddDamage_CRI": return ConvertCallToBonus(call, "bCritAtkRate", ArgumentType.Skip, ArgumentType.Int);
				case "SubDamage_CRI": return ConvertCallToBonus(call, "bCritAtkRate", ArgumentType.Skip, ArgumentType.NegateInt);

				case "AddSPdrain": return ConvertCallToBonus(call, "bSPDrainRate", ArgumentType.IntMult10, ArgumentType.Int);
				case "SubSPdrain": return ConvertCallToBonus(call, "bSPDrainRate", ArgumentType.NegateIntMult10, ArgumentType.Int);

				case "AddHPdrain": return ConvertCallToBonus(call, "bHPDrainRate", ArgumentType.IntMult10, ArgumentType.Int);
				case "SubHPdrain": return ConvertCallToBonus(call, "bHPDrainRate", ArgumentType.NegateIntMult10, ArgumentType.Int);

				case "AddSkillDelay": return ConvertCallToBonus(call, "bSkillCooldown", ArgumentType.Skill, ArgumentType.Int);
				case "SubSkillDelay": return ConvertCallToBonus(call, "bSkillCooldown", ArgumentType.Skill, ArgumentType.NegateInt);

				case "AddDamage_SKID":	return ConvertCallToBonus(call, "bSkillAtk", ArgumentType.Skip, ArgumentType.Skill, ArgumentType.Int);
				case "SubDamage_SKID":	return ConvertCallToBonus(call, "bSkillAtk", ArgumentType.Skip, ArgumentType.Skill, ArgumentType.NegateInt);
				
				case "AddEXPPercent_KillRace": return ConvertCallToBonus(call, "bExpAddRace", ArgumentType.Race, ArgumentType.Int);
				case "SubEXPPercent_KillRace": return ConvertCallToBonus(call, "bExpAddRace", ArgumentType.Race, ArgumentType.NegateInt);

				case "AddReflectMagic": return ConvertCallToBonus(call, "bMagicDamageReturn", ArgumentType.Int);
				case "SubReflectMagic": return ConvertCallToBonus(call, "bMagicDamageReturn", ArgumentType.NegateInt);

				case "AddMeleeAttackReflect": return ConvertCallToBonus(call, "bShortWeaponDamageReturn", ArgumentType.Int);
				case "SubMeleeAttackReflect": return ConvertCallToBonus(call, "bShortWeaponDamageReturn", ArgumentType.NegateInt);

				case "AddSkillMDamage": return ConvertCallToBonus(call, "bMagicAtkEle", ArgumentType.Element, ArgumentType.Int);
				case "SubSkillMDamage": return ConvertCallToBonus(call, "bMagicAtkEle", ArgumentType.Element, ArgumentType.NegateInt);

				case "AddSpecificSpellCastTime": return ConvertCallToBonus(call, "bVariableCastrate", ArgumentType.Skill, ArgumentType.Int);
				case "SubSpecificSpellCastTime": return ConvertCallToBonus(call, "bVariableCastrate", ArgumentType.Skill, ArgumentType.NegateInt);

				case "AddMdamage_Class": return ConvertCallToBonus(call, "bMagicAddClass", ArgumentType.Class, ArgumentType.Int);
				case "SubMdamage_Class": return ConvertCallToBonus(call, "bMagicAddClass", ArgumentType.Class, ArgumentType.NegateInt);

				case "AddGuideAttack": return ConvertCallToBonus(call, "bPerfectHitAddRate", ArgumentType.Int);
				case "SubGuideAttack": return ConvertCallToBonus(call, "bPerfectHitAddRate", ArgumentType.NegateInt);

				case "AddIgnore_RES_RacePercent": return ConvertCallToBonus(call, "bIgnoreResRaceRate", ArgumentType.Race, ArgumentType.Int);
				case "SubIgnore_RES_RacePercent": return ConvertCallToBonus(call, "bIgnoreResRaceRate", ArgumentType.Race, ArgumentType.NegateInt);

				case "AddIgnore_MRES_RacePercent": return ConvertCallToBonus(call, "bIgnoreMResRaceRate", ArgumentType.Race, ArgumentType.Int);
				case "SubIgnore_MRES_RacePercent": return ConvertCallToBonus(call, "bIgnoreMResRaceRate", ArgumentType.Race, ArgumentType.NegateInt);

				case "AddDamage_HIT": return ConvertCallToBonus(call, "bNonCritAtkRate", ArgumentType.Skip, ArgumentType.Int);
				case "SubDamage_HIT": return ConvertCallToBonus(call, "bNonCritAtkRate", ArgumentType.Skip, ArgumentType.NegateInt);

				case "AddCRIPercent_Race": return ConvertCallToBonus(call, "bCriticalAddRace", ArgumentType.Race, ArgumentType.IntDiv10);
				case "SubCRIPercent_Race": return ConvertCallToBonus(call, "bCriticalAddRace", ArgumentType.Race, ArgumentType.NegateIntDiv10);

				case "AddReflectTolerace": return ConvertCallToBonus(call, "bReduceDamageReturn", ArgumentType.NegateInt);
				case "SubReflectTolerace": return ConvertCallToBonus(call, "bReduceDamageReturn", ArgumentType.Int);

				case "SetIgnoreDEFRace":			return ConvertCallToBonus(call, "bIgnoreDefRace", ArgumentType.Race);
				case "SetIgnoreDEFClass":			return ConvertCallToBonus(call, "bIgnoreDefClass", ArgumentType.Class);
				case "SetIgnoreMdefRace":			return ConvertCallToBonus(call, "bIgnoreMdefRaceRate", ArgumentType.Race, ArgumentType.Int);
				case "SetIgnoreMdefClass":			return ConvertCallToBonus(call, "bIgnoreMdefClassRate", ArgumentType.Class, ArgumentType.Int);
				case "SetIgnoreDefRace_Percent":	return ConvertCallToBonus(call, "bIgnoreDefRaceRate", ArgumentType.Race, ArgumentType.Int);
				case "SetIgnoreDefClass_Percent":	return ConvertCallToBonus(call, "bIgnoreDefClassRate", ArgumentType.Class, ArgumentType.Int);

				case "AddNeverknockback": return ConvertCallToBonus(call, "bNoKnockback", ArgumentType.Skip);
				case "NoDispell": return ConvertCallToBonus(call, "bNoCastCancel", ArgumentType.Skip);
				case "IsPremiumPcCafe": return ConvertIsPremiumPcCafe(call);
				case "Condition": return ConvertCondition(call);
				case "AddHealValue": return ConvertCallToBonus(call, "bHealPower", ArgumentType.Int);
				case "EnableSkill": return ConvertCallToFunction(call, "skill", ArgumentType.Skill, ArgumentType.Int);
				case "GetSkillLevel": return ConvertCallToFunction(call, "getskilllv", ArgumentType.Skill);
				case "AddReceiveItem_Equip": return ConvertAddReceiveItem_Equip(call);
				case "PerfectDamage":	return ConvertCallToBonus(call, "bNoSizeFix", ArgumentType.Skip);
				case "SplashAttack":	return ConvertCallToBonus(call, "bSplashRange", ArgumentType.Int);
				case "AddBowAttackDamage":	return ConvertAddBowAttackDamage(call);
				case "NoJamstone": return new AthenaCallExpression("bNoGemStone");
				case "NoMadogearfuel": return new AthenaCallExpression("bNoMadoFuel");
				case "Reincarnation": return new AthenaCallExpression("bRestartFullRecover");
				case "Clairvoyance": return new AthenaCallExpression("bIntravision");
				case "Magicimmune": return ConvertMagicImmune(call);
				case "SetInvestigate": return ConvertSetInvestigate(call);

				case "GetPureJob": return new AthenaLiteralExpression("BaseLevel", true);
				case "GetPetRelationship": return new AthenaLiteralExpression("getpetinfo(PETINFO_INTIMATE)", true);
				case "GetWeaponClass": return new AthenaLiteralExpression("getiteminfo(getequipid(), ITEMINFO_VIEW)", true);
				case "GetRefineLevel": return ConvertEqiFunction(call, "getrefine");
				case "GetEquipGradeLevel": return ConvertEqiFunction(call, "getenchantgrade");
				case "GetEquipWeaponLv": return ConvertEqiFunction(call, "getequipweaponlv");
				case "GetEquipArmorLv": return ConvertEqiFunction(call, "getequiparmorlv");
				case "GetItemIDLocation": return ConvertEqiFunction(call, "getequiprefinerycnt");

				case "AddSFCTEquipPermill": return ConvertCallToBonus(call, "bFixedCastrate", ArgumentType.Skip, ArgumentType.IntDiv10, ArgumentType.Skip);
				case "SubSFCTEquipPermill": return ConvertCallToBonus(call, "bFixedCastrate", ArgumentType.Skip, ArgumentType.NegateIntDiv10, ArgumentType.Skip);

				case "SetEquipTempValue": return new NullExpression();

				case "get": return ConvertGet(call);
				default:
					return ConvertCall(call);
					throw new NotSupportedException($"Unknown Lua function: {identifier.Name}.");
			}
		}

		public enum Unit {
			User,
			Target,
		}

		private bool IsUnit(LuaCallExpression call, int index, Unit unit) {
			var arg = call.Arguments[index];

			if (arg is LuaLiteralExpression literal && literal.Type == LuaLiteralType.Number) {
				return (int)(double)literal.Value == (int)unit;
			}

			throw new Exception($"Expected an integer for argument #{index} with method {((LuaIdentifierExpression)call.Target).Name}.");
		}

		private AthenaExpression ConvertEqiFunction(LuaCallExpression call, string name) {
			ArgumentCheck(call, 1);

			AthenaCallExpression result = new AthenaCallExpression(name);
			var arg0 = call.Arguments[0];

			if (arg0 is LuaLiteralExpression literal && literal.Type == LuaLiteralType.Number) {
				result.Arguments.Add(new AthenaVariableExpression(EquipIdToEqiLocation((int)(double)literal.Value)));
			}

			return result;
		}

		private AthenaExpression ConvertSetInvestigate(LuaCallExpression call) {
			ArgumentCheck(call, 0);

			AthenaCallExpression result = new AthenaCallExpression("bonus");
			result.Arguments.Add(new AthenaVariableExpression("bDefRatioAtkClass"));
			result.Arguments.Add(new AthenaVariableExpression("Class_All"));
			return result;
		}

		private AthenaExpression ConvertMagicImmune(LuaCallExpression call) {
			ArgumentCheck(call, 1);

			AthenaCallExpression result = new AthenaCallExpression("bonus");
			result.Arguments.Add(new AthenaVariableExpression("bNoMagicDamage"));
			result.Arguments.Add(new AthenaVariableExpression("100"));
			return result;
		}

		private AthenaExpression ConvertAddBowAttackDamage(LuaCallExpression call, bool negate = false) {
			ArgumentCheck(call, 2);

			AthenaCallExpression result = new AthenaCallExpression("bonus2");
			result.Arguments.Add(new AthenaVariableExpression("bWeaponDamageRate"));
			result.Arguments.Add(new AthenaVariableExpression("W_BOW"));
			result.Arguments.Add(ConvertExpression(call.Arguments[1]));
			return result;
		}

		private AthenaExpression ConvertSkillSpConsumption(LuaCallExpression call, bool negate = false) {
			ArgumentCheck(call, 2);

			AthenaCallExpression result = new AthenaCallExpression("bonus2");

			AddArgument(call, result, ArgumentType.Skill, 1);
			
			if (negate)
				AddArgument(call, result, ArgumentType.Int, 0);
			else
				AddArgument(call, result, ArgumentType.NegateInt, 0);

			return result;
		}

		private AthenaExpression ConvertCondition(LuaCallExpression call) {
			ArgumentCheck(call, 3);

			var arg0 = (int)((AthenaLiteralExpression)ConvertExpression(call.Arguments[0])).NumberValue;
			var arg1 = (int)((AthenaLiteralExpression)ConvertExpression(call.Arguments[1])).NumberValue;
			var arg2 = (int)((AthenaLiteralExpression)ConvertExpression(call.Arguments[2])).NumberValue;

			AthenaCallExpression result = new AthenaCallExpression();

			switch (arg0) {
				case 13:
					result.Name = "bNoWalkDelay";
					return result;
				case 14:
					result.Name = "bSpeedRate";
					result.Arguments.Add(new AthenaLiteralExpression(25));
					return result;
				default:
					throw new Exception($"Unrecognized parameter for Condition: {arg0}.");
			}
		}

		private AthenaExpression ConvertIsPremiumPcCafe(LuaCallExpression call) {
			ArgumentCheck(call, 0);

			AthenaCallExpression result = new AthenaCallExpression("vip_status");
			result.Arguments.Add(new AthenaVariableExpression("VIP_STATUS_ACTIVE"));
			return result;
		}

		public enum ArgumentType {
			Skip,
			Int,
			NegateInt,
			Size,
			Skill,
			Race,
			Element,
			Class,
			BattleFlag,
			UserTarget,
			IntDiv10,
			NegateIntDiv10,
			IntMult10,
			NegateIntMult10,
		}

		private AthenaExpression ConvertGet(LuaCallExpression call) {
			ArgumentCheck(call, 1);

			AthenaCallExpression result = new AthenaCallExpression("readparam");

			var arg1 = ConvertExpression(call.Arguments[0]);

			if (!(arg1 is AthenaLiteralExpression lit) || lit.Type != AthenaLiteralType.Number)
				throw new Exception();

			var spId = (int)lit.NumberValue;
			string spBonusName = "";

			switch (spId) {
				case 32: spBonusName = "bStr"; break;
				case 33: spBonusName = "bAgi"; break;
				case 34: spBonusName = "bVit"; break;
				case 35: spBonusName = "bInt"; break;
				case 36: spBonusName = "bDex"; break;
				case 37: spBonusName = "bLuk"; break;

				case 255: spBonusName = "bPow"; break;
				case 256: spBonusName = "bSta"; break;
				case 257: spBonusName = "bWis"; break;
				case 258: spBonusName = "bSpl"; break;
				case 259: spBonusName = "bCon"; break;
				case 260: spBonusName = "bCrt"; break;

				case 263: spBonusName = "bCrt"; break;
				case 264: spBonusName = "bCrt"; break;

				case 11:
					return new AthenaLiteralExpression("BaseLevel", true);
				case 19:
					return new AthenaLiteralExpression("BaseJob", true);
				case 55:
					return new AthenaLiteralExpression("JobLevel", true);

				default:
					throw new Exception($"Unrecognized parameter for get({spId}).");
			}

			result.Arguments.Add(new AthenaVariableExpression(spBonusName));
			return result;
		}

		private AthenaExpression ConvertAddExtParam(LuaCallExpression call, ArgumentType type = ArgumentType.Int) {
			ArgumentCheck(call, 3);

			AthenaCallExpression result = new AthenaCallExpression("bonus");

			var arg1 = ConvertExpression(call.Arguments[1]);

			if (!(arg1 is AthenaLiteralExpression lit) || lit.Type != AthenaLiteralType.Number)
				throw new Exception();

			var spId = (int)lit.NumberValue;
			string spBonusName = "";

			switch (spId) {
				case 41: spBonusName = "bBaseAtk"; break;
				case 45: spBonusName = "bDef"; break;
				case 47: spBonusName = "bMdef"; break;
				case 49: spBonusName = "bHit"; break;

				case 50: spBonusName = "bFlee"; break;
				case 51: spBonusName = "bFlee2"; break;
				case 52:
					spBonusName = "bCritical";
					type = type == ArgumentType.NegateInt ? ArgumentType.NegateIntDiv10 : ArgumentType.IntDiv10;
					break;

				case 54: spBonusName = "bAspd"; break;

				case 103: spBonusName = "bStr"; break;
				case 104: spBonusName = "bAgi"; break;
				case 105: spBonusName = "bVit"; break;
				case 106: spBonusName = "bInt"; break;
				case 107: spBonusName = "bDex"; break;
				case 108: spBonusName = "bLuk"; break;
				case 109: spBonusName = "bMaxHP"; break;
				case 110: spBonusName = "bMaxSP"; break;
				case 111: spBonusName = "bMaxHPrate"; break;
				case 112: spBonusName = "bMaxSPrate"; break;
				case 113: spBonusName = "bHPrecovRate"; break;
				case 114: spBonusName = "bSPrecovRate"; break;
				
				case 140: spBonusName = "bMatkRate"; break;

				case 167: spBonusName = "bAspdRate"; break;

				case 200: spBonusName = "bMatk"; break;

				case 207: spBonusName = "bAtkRate"; break;

				case 234: spBonusName = "bPow"; break;
				case 235: spBonusName = "bSta"; break;
				case 236: spBonusName = "bWis"; break;
				case 237: spBonusName = "bSpl"; break;
				case 238: spBonusName = "bCon"; break;
				case 239: spBonusName = "bCrt"; break;

				case 242: spBonusName = "bPatk"; break;
				case 243: spBonusName = "bSmatk"; break;
				case 244: spBonusName = "bRes"; break;
				case 245: spBonusName = "bMres"; break;
				case 253: spBonusName = "bCrate"; break;
				case 254: spBonusName = "bHplus"; break;

				default:
					throw new Exception($"Unrecognized parameter (SP_BONUS) for AddExtParam: {spId}.");
			}

			result.Arguments.Add(new AthenaVariableExpression(spBonusName));

			AddArgument(call, result, type, 2);
			return result;
		}

		private AthenaExpression ConvertCallToFunction(LuaCallExpression call, string athenaName) {
			AthenaCallExpression result = new AthenaCallExpression(athenaName);

			foreach (var argument in call.Arguments) {
				result.Arguments.Add(ConvertExpression(argument));
			}

			return result;
		}

		private AthenaExpression ConvertAddReceiveItem_Equip(LuaCallExpression call) {
			ArgumentCheck(call, 1);

			AthenaCallExpression result = new AthenaCallExpression("bDropAddRace");

			result.Arguments.Add(new AthenaLiteralExpression("RC_All"));
			result.Arguments.Add(ConvertExpression(call.Arguments[0]));

			return result;
		}

		private AthenaExpression ConvertCallToFunction(LuaCallExpression call, string athenaName, params ArgumentType[] argumentTypes) {
			ArgumentCheck(call, argumentTypes.Length);

			AthenaCallExpression result = new AthenaCallExpression(athenaName);

			AddArguments(call, result, argumentTypes);

			return result;
		}

		private AthenaExpression ConvertCallToBonus(LuaCallExpression call, string athenaName, params ArgumentType[] argumentTypes) {
			ArgumentCheck(call, argumentTypes.Length);

			AthenaCallExpression result = new AthenaCallExpression();

			AddArguments(call, result, argumentTypes);

			result.Name = "bonus" + (result.Arguments.Count > 1 ? result.Arguments.Count.ToString() : "");
			result.Arguments.Insert(0, new AthenaVariableExpression(athenaName));

			return result;
		}

		private void AddArgument(LuaCallExpression call, AthenaCallExpression result, ArgumentType type, int index) {
			switch (type) {
				case ArgumentType.Skip:
					return;
				case ArgumentType.Skill:
					var skill = ConvertExpression(call.Arguments[index]);

					if (skill is AthenaLiteralExpression literal && literal.Type == AthenaLiteralType.Number) {
						int skillId = (int)literal.NumberValue;
						var skillDb = SdeEditor.Project.GetDb(DataSources.Skill);
						var skillTuple = skillDb.Table.TryGetTuple(skillId);

						if (skillTuple != null) {
							skill = new AthenaLiteralExpression(skillTuple.GetModel<Skill>().Name);
						}
					}

					result.Arguments.Add(skill);
					break;
				case ArgumentType.Int:
					result.Arguments.Add(ConvertExpression(call.Arguments[index]));
					break;
				case ArgumentType.NegateInt:
					result.Arguments.Add(Negate(ConvertExpression(call.Arguments[index])));
					break;
				case ArgumentType.Size:
					result.Arguments.Add(ConvertArgumentToType<SizeType>(call.Arguments[index], "Size_"));
					break;
				case ArgumentType.Race:
					result.Arguments.Add(ConvertArgumentToType<RaceType>(call.Arguments[index], "RC_"));
					break;
				case ArgumentType.Element:
					result.Arguments.Add(ConvertArgumentToType<ElementType>(call.Arguments[index], "Ele_"));
					break;
				case ArgumentType.Class:
					result.Arguments.Add(ConvertArgumentToType<ClassType>(call.Arguments[index], "Class_"));
					break;
				case ArgumentType.BattleFlag:
					result.Arguments.Add(ConvertArgumentToType<BattleFlagType>(call.Arguments[index], "BF_"));
					break;
				case ArgumentType.IntDiv10:
					result.Arguments.Add(Div(ConvertExpression(call.Arguments[index]), 10));
					break;
				case ArgumentType.NegateIntDiv10:
					result.Arguments.Add(Div(Negate(ConvertExpression(call.Arguments[index])), 10));
					break;
				case ArgumentType.IntMult10:
					result.Arguments.Add(Mult(ConvertExpression(call.Arguments[index]), 10));
					break;
				case ArgumentType.NegateIntMult10:
					result.Arguments.Add(Mult(Negate(ConvertExpression(call.Arguments[index])), 10));
					break;
			}
		}

		private void AddArguments(LuaCallExpression call, AthenaCallExpression result, ArgumentType[] argumentTypes) {
			for (int i = 0; i < argumentTypes.Length; i++) {
				AddArgument(call, result, argumentTypes[i], i);
			}
		}

		public AthenaExpression ConvertArgumentToType<TEnum>(LuaExpression luaExpression, string prefix) where TEnum : struct, Enum {
			var expression = ConvertExpression(luaExpression);

			if (expression is AthenaLiteralExpression literal && literal.Type == AthenaLiteralType.Number) {
				int value = (int)literal.NumberValue;

				try {
					if (value == 9999) {
						var tEnum = DbReader.LoadEnum("All", default(TEnum));
						var enumInfo = EnumInfos.GetEnumBase<TEnum>(tEnum);
						return new AthenaLiteralExpression(prefix + enumInfo.YamlName, true);
					}
					else {
						var enumInfo = EnumInfos.GetEnumBase<TEnum>((TEnum)(object)value);
						return new AthenaLiteralExpression(prefix + enumInfo.YamlName, true);
					}
				}
				catch {
					return expression;
				}
			}

			return expression;
		}

		public static string ArgumentToType<TEnum>(int value, string prefix) where TEnum : struct, Enum {
			var enumInfo = EnumInfos.GetEnumBase<TEnum>((TEnum)(object)value);
			return prefix + enumInfo.YamlName; ;
		}

		private AthenaStatement ConvertExpressionStatement(LuaExpressionStatement statement) {
			return new AthenaExpressionStatement(ConvertExpression(statement.Expression));
		}

		private AthenaExpression Negate(AthenaExpression expression) {
			return new AthenaUnaryExpression {
				Operator = LuaUnaryOperator.Negate,
				Operand = expression
			};
		}

		private AthenaExpression Div(AthenaExpression expression, int value) {
			return new AthenaBinaryExpression(expression, LuaBinaryOperator.Divide, new AthenaLiteralExpression(value));
		}

		private AthenaExpression Mult(AthenaExpression expression, int value) {
			return new AthenaBinaryExpression(expression, LuaBinaryOperator.Multiply, new AthenaLiteralExpression(value));
		}

		public string EquipIdToEqiLocation(int equipId) {
			switch (equipId) {
				case 2: return "EQI_ARMOR";
				case 3: return "EQI_HAND_L";
				case 4: return "EQI_HAND_R";
				case 5: return "EQI_GARMENT";
				case 6: return "EQI_SHOES";
				case 10: return "EQI_HEAD_TOP";
				case 11: return "EQI_HEAD_MID";
				case 12: return "EQI_HEAD_LOW";
				case 30: return "EQI_SHADOW_ARMOR";
				case 31: return "EQI_SHADOW_WEAPON";
				case 32: return "EQI_SHADOW_SHIELD";
				case 33: return "EQI_SHADOW_SHOES";
				case 34: return "EQI_SHADOW_ACC_R";
				case 35: return "EQI_SHADOW_ACC_L";
				default:
					throw new Exception($"Unrecognized RO equipment id: {equipId}.");
			}
		}
	}
}
