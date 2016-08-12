using System;
using System.Linq;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class CellsCompiledFormula : ICompiledFormula
	{
		private readonly string _reference;
		private readonly string _targets;

		private CellsCompiledFormula(string reference, string targets)
		{
			_reference = reference;
			_targets = targets;
		}

		public string GetFormulaText()
		{
			return _targets;
		}

		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as CellsCommandDef;
			Contract.Requires<ArgumentNullException>(def != null);

			var sheetContextResult = context.GetSheetContext(def.SheetName);
			if (sheetContextResult.Failure)
			{
				return Result.Fail<ICompiledFormula>(sheetContextResult.Error);
			}

			var filterResult = context.CompileFilter(def);
			if (filterResult.Failure)
			{
				return Result.Fail<ICompiledFormula>(filterResult.Error);
			}

			var sheet = sheetContextResult.Value.ContextualPrefix;
			var targetFormulas = sheetContextResult.Value.GetByIdInScope(def.Reference, filterResult.Value, def.Scope);
			var targets = string.Join(", ", targetFormulas.Select(f => sheet + f.Reference).ToArray());

			var compiledFormula = new CellsCompiledFormula(def.Reference, targets);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}