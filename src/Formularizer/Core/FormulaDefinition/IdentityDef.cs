using System;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
	public class IdentityDef : IFormulaPartDef
	{
		private IdentityDef(string id, string reference)
		{
			Reference = reference;
			Id = id;
		}

		public string Id { get; private set; }
		public string Reference { get; private set; }

		public static Result<IFormulaPartDef> Create(string rawText, string reference)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(rawText));

			var parts = rawText.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2)
			{
				return Result.Fail<IFormulaPartDef>("Incorrect format of formula's identity.");
			}

			if (!"id".Equals(parts[0].Trim().ToLower(), StringComparison.InvariantCultureIgnoreCase))
			{
				return Result.Fail<IFormulaPartDef>("Not formula identity command.");
			}

			var trimmedId = parts[1].Trim();

			if (string.IsNullOrEmpty(trimmedId))
			{
				return Result.Fail<IFormulaPartDef>("No id specified for identity command.");
			}

			return Result.Ok((IFormulaPartDef) new IdentityDef(trimmedId, reference));
		}
	}
}