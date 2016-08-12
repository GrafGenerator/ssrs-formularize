using System;
using System.Linq;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class CellCompiledFormula : ICompiledFormula
	{
		private readonly string _reference;
		private readonly string _target;

		private CellCompiledFormula(string reference, string target)
		{
			_reference = reference;
			_target = target;
		}

		public string GetFormulaText()
		{
			return _target;
		}

		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as CellCommandDef;
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

			var targetFormula =
				sheetContextResult.Value.GetByIdInScope(def.Reference, filterResult.Value, def.Scope).FirstOrDefault();
			if (targetFormula == null)
			{
				return Result.Fail<ICompiledFormula>(string.Format("Target formula '{0}' not found", filterResult.Value));
			}

			var target = targetFormula != null ? targetFormula.Reference : "";
			var sheet = sheetContextResult.Value.ContextualPrefix;

			var compiledFormula = new CellCompiledFormula(def.Reference, sheet + target);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}