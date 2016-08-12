using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
	public class FormulaDef : IFormulaPartContainerDef
	{
		private FormulaDef(IEnumerable<IFormulaPartDef> parts, string reference)
		{
			Reference = reference;
			Parts = parts;
		}

		public IEnumerable<IFormulaPartDef> Parts { get; private set; }
		public string Reference { get; private set; }

		public static Result<IFormulaPartDef> Create(IEnumerable<IFormulaPartDef> parts, string reference)
		{
			Contract.Requires<ArgumentNullException>(parts != null);

			var formulaPartDefs = parts as IFormulaPartDef[] ?? parts.ToArray();
			if (!formulaPartDefs.Any())
			{
				return Result.Fail<IFormulaPartDef>("No formula parts provided.");
			}

			var hasSelfLinkingCommand = formulaPartDefs.OfType<SelectorCommandDef>().Any(c => c.UseOwnId);
			var hasIdentity = formulaPartDefs.OfType<IdentityDef>().Any();

			if (hasSelfLinkingCommand && !hasIdentity)
			{
				return Result.Fail<IFormulaPartDef>("Formula assumes using of own identity, but no identity provided.");
			}

			return Result.Ok((IFormulaPartDef) new FormulaDef(formulaPartDefs, reference));
		}
	}
}