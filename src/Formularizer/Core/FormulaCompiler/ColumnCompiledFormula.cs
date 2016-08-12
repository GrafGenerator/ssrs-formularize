using System;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class ColumnCompiledFormula : ICompiledFormula
	{
		private readonly string _reference;
		private readonly string _target;

		private ColumnCompiledFormula(string reference, string target)
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
			var def = definition as ColumnCommandDef;
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
			var targetFormula = sheetContextResult.Value.GetById(filterResult.Value).FirstOrDefault();
			var targetColumn = targetFormula != null ? Utility.SplitReference(targetFormula.Reference)[0] : "";
			var target = sheet + targetColumn + ":" + targetColumn;

			var compiledFormula = new ColumnCompiledFormula(def.Reference, target);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}