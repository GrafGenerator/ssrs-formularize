using System;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class StaticTextCompiledFormula : ICompiledFormula
	{
		private readonly string _rawFormulaText;

		private StaticTextCompiledFormula(string rawFormulaText)
		{
			_rawFormulaText = rawFormulaText;
		}

		public string GetFormulaText()
		{
			return _rawFormulaText;
		}


		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as StaticTextDef;
			Contract.Requires<ArgumentException>(def != null);

			var compiledFormula = new StaticTextCompiledFormula(def.Content);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}