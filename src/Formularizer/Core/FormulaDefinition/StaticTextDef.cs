using System;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
	public class StaticTextDef : IFormulaPartDef
	{
		private StaticTextDef(string rawText, string reference)
		{
			Reference = reference;
			Content = rawText;
		}

		public string Content { get; private set; }
		public string Reference { get; private set; }

		public static Result<IFormulaPartDef> Create(string rawText, string reference)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(rawText));

			return Result.Ok((IFormulaPartDef) new StaticTextDef(rawText, reference));
		}
	}
}