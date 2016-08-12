using System;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class SelfReferenceCompiledFormula : ICompiledFormula
	{
		private readonly string _reference;

		private SelfReferenceCompiledFormula(string reference)
		{
			_reference = reference;
		}

		public string GetFormulaText()
		{
			return _reference;
		}

		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as SelfReferenceCommandDef;
			Contract.Requires<ArgumentException>(def != null);

			var compiledFormula = new SelfReferenceCompiledFormula(def.Reference);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}