using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaBuilder;
using Formularizer.Core.FormulaDefinition;
using NUnit.Framework;

namespace FormularizerTests
{
	[TestFixture]
	public class FormulaBuilderFixture
	{
		[TestCase("=static text", Result = "[static]")]
		[TestCase("        ", Result = "[static]")]
		[TestCase("235948523984", Result = "[static]")]
		[TestCase("цшгкпрзукшпукшгп", Result = "[static]")]
		[TestCase("!#$%$!@&^&@(@+_+__*()&`", Result = "[static]")]
		public string Match_Static_Part(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{id=1}", Result = "[identity(1)]")]
		[TestCase("{id=   1}", Result = "[identity(1)]")]
		[TestCase("{id=1    }", Result = "[identity(1)]")]
		[TestCase("{id=      1    }", Result = "[identity(1)]")]
		[TestCase("{id   =      1    }", Result = "[identity(1)]")]
		[TestCase("{        id   =      1    }", Result = "[identity(1)]")]
		[TestCase("{id=abc}", Result = "[identity(abc)]")]
		[TestCase("{id=abc def}", Result = "[identity(abc def)]")]
		[TestCase("{id=щгуцкап}", Result = "[identity(щгуцкап)]")]
		[TestCase("{id=щгу, цкап}", Result = "[identity(щгу, цкап)]")]
		public string Match_Identity(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{this}", Result = "[this('')]")]
		[TestCase("{this'1'}", Result = "[this('1')]")]
		[TestCase("{this'1*'}", Result = "[this('1*')]")]
		[TestCase("{this'1_'}", Result = "[this('1_')]")]
		[TestCase("{this'1+'}", Result = "[this('1+')]")]
		[TestCase("{this'1-'}", Result = "[this('1-')]")]
		[TestCase("{this '1'}", Result = "[this('1')]")]
		[TestCase("{this '   1'}", Result = "[this('   1')]")]
		[TestCase("{this '1   '}", Result = "[this('1   ')]")]
		[TestCase("{this 'abc'}", Result = "[this('abc')]")]
		public string Match_Self_Reference_Command(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{cells'1'column}", Result = "[cells('1', c, '', false)]")]
		[TestCase("{cells '1' column}", Result = "[cells('1', c, '', false)]")]
		[TestCase("{cells '1' column 'sheet'}", Result = "[cells('1', c, 'sheet', false)]")]
		[TestCase("{cells '1' column 'sheet.with.dots'}", Result = "[cells('1', c, 'sheet.with.dots', false)]")]
		[TestCase("{cells '1' column 'sheet (with parentheses)'}",
			Result = "[cells('1', c, 'sheet (with parentheses)', false)]")]
		[TestCase("{cells '1  ' column 'sheet'}", Result = "[cells('1  ', c, 'sheet', false)]")]
		[TestCase("{cells '  1' column 'sheet'}", Result = "[cells('  1', c, 'sheet', false)]")]
		[TestCase("{cells '1' column '  sheet'}", Result = "[cells('1', c, '  sheet', false)]")]
		[TestCase("{cells '1' column 'sheet  '}", Result = "[cells('1', c, 'sheet  ', false)]")]
		[TestCase("{cells '1'   column  'sheet'}", Result = "[cells('1', c, 'sheet', false)]")]
		[TestCase("{cells '1' row}", Result = "[cells('1', r, '', false)]")]
		[TestCase("{cells '1' all}", Result = "[cells('1', a, '', false)]")]
		//[TestCase("{cells '1*' column}", Result = "[cells('1*', c, '', true)]")] // move to complex checks
		[TestCase("{cells '1_' column}", Result = "[cells('1_', c, '', false)]")]
		[TestCase("{cells '1+' column}", Result = "[cells('1+', c, '', false)]")]
		[TestCase("{cells '1-' column}", Result = "[cells('1-', c, '', false)]")]
		[TestCase("{cells '1,' column}", Result = "[cells('1,', c, '', false)]")]
		[TestCase("{cells '1.' column}", Result = "[cells('1.', c, '', false)]")]
		public string Match_Cells_Command(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{cell'1'column}", Result = "[cell('1', c, '', false)]")]
		[TestCase("{cell '1' column}", Result = "[cell('1', c, '', false)]")]
		[TestCase("{cell '1' column 'sheet'}", Result = "[cell('1', c, 'sheet', false)]")]
		[TestCase("{cell '1' column 'sheet.with.dots'}", Result = "[cell('1', c, 'sheet.with.dots', false)]")]
		[TestCase("{cell '1' column 'sheet (with parentheses)'}", Result = "[cell('1', c, 'sheet (with parentheses)', false)]"
			)]
		[TestCase("{cell '1  ' column 'sheet'}", Result = "[cell('1  ', c, 'sheet', false)]")]
		[TestCase("{cell '  1' column 'sheet'}", Result = "[cell('  1', c, 'sheet', false)]")]
		[TestCase("{cell '1' column '  sheet'}", Result = "[cell('1', c, '  sheet', false)]")]
		[TestCase("{cell '1' column 'sheet  '}", Result = "[cell('1', c, 'sheet  ', false)]")]
		[TestCase("{cell '1'   column  'sheet'}", Result = "[cell('1', c, 'sheet', false)]")]
		[TestCase("{cell '1' row}", Result = "[cell('1', r, '', false)]")]
		[TestCase("{cell '1' all}", Result = "[cell('1', a, '', false)]")]
		//[TestCase("{cell '1*' column}", Result = "[cell('1*', c, '', true)]")] // move to complex checks
		[TestCase("{cell '1_' column}", Result = "[cell('1_', c, '', false)]")]
		[TestCase("{cell '1+' column}", Result = "[cell('1+', c, '', false)]")]
		[TestCase("{cell '1-' column}", Result = "[cell('1-', c, '', false)]")]
		[TestCase("{cell '1,' column}", Result = "[cell('1,', c, '', false)]")]
		[TestCase("{cell '1.' column}", Result = "[cell('1.', c, '', false)]")]
		public string Match_Cell_Command(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{column'1'}", Result = "[column('1', '', false)]")]
		[TestCase("{column '1'}", Result = "[column('1', '', false)]")]
		[TestCase("{column '1''sheet'}", Result = "[column('1', 'sheet', false)]")]
		[TestCase("{column '1' 'sheet'}", Result = "[column('1', 'sheet', false)]")]
		[TestCase("{column '1' 'sheet.with.dots'}", Result = "[column('1', 'sheet.with.dots', false)]")]
		[TestCase("{column '1' 'sheet (with parentheses)'}", Result = "[column('1', 'sheet (with parentheses)', false)]")]
		[TestCase("{column '1  ' 'sheet'}", Result = "[column('1  ', 'sheet', false)]")]
		[TestCase("{column '  1' 'sheet'}", Result = "[column('  1', 'sheet', false)]")]
		[TestCase("{column '1' '  sheet'}", Result = "[column('1', '  sheet', false)]")]
		[TestCase("{column '1' 'sheet  '}", Result = "[column('1', 'sheet  ', false)]")]
		[TestCase("{column '1'    'sheet'}", Result = "[column('1', 'sheet', false)]")]
		//[TestCase("{column '1*'}", Result = "[column('1*', '', true)]")] // move to complex checks
		[TestCase("{column '1_'}", Result = "[column('1_', '', false)]")]
		[TestCase("{column '1+'}", Result = "[column('1+', '', false)]")]
		[TestCase("{column '1-'}", Result = "[column('1-', '', false)]")]
		[TestCase("{column '1,'}", Result = "[column('1,', '', false)]")]
		[TestCase("{column '1.'}", Result = "[column('1.', '', false)]")]
		public string Match_Column_Command(string rawFormula)
		{
			return RunFormula(rawFormula);
		}

		[TestCase("{id=1}={cells '1*' column}", Result = "[identity(1), static, cells('1*', c, '', true)]")]
		[TestCase("{id=1}={cell '1*' column}", Result = "[identity(1), static, cell('1*', c, '', true)]")]
		[TestCase("{id=1}={column '1*'}", Result = "[identity(1), static, column('1*', '', true)]")]
		public string Match_Commands_With_Own_Identity(string rawFormula)
		{
			return RunFormula(rawFormula);
		}


		[TestCase("{id=1}=SUM({column 'columnB' 'sheet1'}; {this}; {column 'columnK' 'sheet1'})",
			Result =
				"[identity(1), static, column('columnB', 'sheet1', false), static, this(''), static, column('columnK', 'sheet1', false), static]"
			)]
		public string Different_Complex_Cases(string rawFormula)
		{
			return RunFormula(rawFormula);
		}


		private string RunFormula(string rawFormula)
		{
			var builder = new FormulaDefinitionBuilder();
			var formulaDef = builder.BuildDef(FormulaInfo.Create("A1", rawFormula));

			Assert.That(formulaDef.Success, Is.True, formulaDef.Error);
			return Dump(formulaDef.Value);
		}

		private string Dump(IFormulaPartDef def)
		{
			if (def == null) return "";

			var container = def as IFormulaPartContainerDef;
			if (container != null)
			{
				var partDefs = container.Parts.Select(Dump);
				return "[" + string.Join(", ", partDefs.ToArray()) + "]";
			}

			var staticText = def as StaticTextDef;
			if (staticText != null)
			{
				return "static";
			}

			var identity = def as IdentityDef;
			if (identity != null)
			{
				return string.Format("identity({0})", identity.Id);
			}

			var cellsCommandDef = def as CellsCommandDef;
			if (cellsCommandDef != null)
			{
				return string.Format("cells('{0}', {1}, '{2}', {3})", cellsCommandDef.Filter,
					GetScope(cellsCommandDef.Scope), cellsCommandDef.SheetName,
					cellsCommandDef.UseOwnId.ToString().ToLower());
			}

			var cellCommandDef = def as CellCommandDef;
			if (cellCommandDef != null)
			{
				return string.Format("cell('{0}', {1}, '{2}', {3})", cellCommandDef.Filter,
					GetScope(cellCommandDef.Scope), cellCommandDef.SheetName,
					cellCommandDef.UseOwnId.ToString().ToLower());
			}

			var columnCommandDef = def as ColumnCommandDef;
			if (columnCommandDef != null)
			{
				return string.Format("column('{0}', '{1}', {2})", columnCommandDef.Filter,
					columnCommandDef.SheetName, columnCommandDef.UseOwnId.ToString().ToLower());
			}

			var selfReferenceCommand = def as SelfReferenceCommandDef;
			if (selfReferenceCommand != null)
			{
				return string.Format("this('{0}')", selfReferenceCommand.SheetName);
			}

			return "unknown";
		}


		private static string GetScope(SelectorScope scope)
		{
			switch (scope)
			{
				case SelectorScope.Column:
					return "c";
				case SelectorScope.Row:
					return "r";
				case SelectorScope.All:
					return "a";
				default:
					return "unknown";
			}
		}
	}
}