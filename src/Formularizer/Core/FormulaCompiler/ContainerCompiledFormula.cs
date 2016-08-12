using System;
using System.Linq;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class ContainerCompiledFormula : ICompiledFormula
	{
		private readonly string _formulaText;

		private ContainerCompiledFormula(string formulaText)
		{
			_formulaText = formulaText;
		}

		public string GetFormulaText()
		{
			return _formulaText;
		}

		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as IFormulaPartContainerDef;
			Contract.Requires<ArgumentException>(def != null);

			var newContext = context.EnterFormula();

			var compiledParts = def.Parts.Select(newContext.CompileFormula);
			var failedPart = compiledParts.FirstOrDefault(c => c.Failure);

			if (failedPart != null)
			{
				return Result.Fail<ICompiledFormula>(failedPart.Error);
			}

			var compiledFormula =
				new ContainerCompiledFormula(string.Join("", compiledParts.Select(p => p.Value.GetFormulaText()).ToArray()));
			newContext.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}