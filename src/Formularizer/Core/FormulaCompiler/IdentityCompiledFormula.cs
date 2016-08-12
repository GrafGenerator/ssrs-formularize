using System;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class IdentityCompiledFormula : ICompiledFormula
	{
		private IdentityCompiledFormula(string id)
		{
			Id = id;
		}

		public string Id { get; private set; }

		public string GetFormulaText()
		{
			return string.Empty;
		}


		public static Result<ICompiledFormula> Compile(SheetResolvingContext context, IFormulaPartDef definition)
		{
			var def = definition as IdentityDef;
			Contract.Requires<ArgumentException>(def != null);

			var compiledFormula = new IdentityCompiledFormula(def.Id);
			context.AppendFormula(compiledFormula);
			return Result.Ok((ICompiledFormula) compiledFormula);
		}
	}
}