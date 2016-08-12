using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaBuilder
{
	public class FormulaDefinitionBuilder
	{
		public Result<IFormulaPartDef> BuildDef(FormulaInfo<string> formula)
		{
			return SplitRawFormula(formula.Value)
				.OnSuccess(parts =>
				{
					var factory = new FormulaSignatureMatchingFactory();
					var partsResults = parts.Select(p => factory.MatchDef(p, formula.Reference)).ToList();
					var failedPart = partsResults.FirstOrDefault(p => p.Failure);

					if (failedPart != null)
					{
						return Result.Fail<IFormulaPartDef>(failedPart.Error);
					}

					return FormulaDef.Create(partsResults.Select(p => p.Value), formula.Reference);
				})
				.OnBoth(result => result);
		}

		private Result<List<string>> SplitRawFormula(string rawFormula)
		{
			var parts = new List<string>();
			var startIndex = 0;
			var endIndex = 0;
			var l = rawFormula.Length;

			while (startIndex < l && endIndex < l)
			{
				var isCommand = rawFormula[startIndex].Equals(Constants.FormulaStartChar);
				var blockEndChar = isCommand ? Constants.FormulaEndChar : Constants.FormulaStartChar;

				while (endIndex < l && !rawFormula[endIndex].Equals(blockEndChar)) endIndex++;
				if (!isCommand) endIndex--;

				parts.Add(rawFormula.Substring(startIndex, endIndex - startIndex + 1));

				startIndex = ++endIndex;
			}

			// check correctness
			foreach (var part in parts)
			{
				var trimmed = part.Trim();
				var isCommand = trimmed.StartsWith(Constants.FormulaStartSymbol) &&
				                trimmed.EndsWith(Constants.FormulaEndSymbol);
				var isStatic = !trimmed.StartsWith(Constants.FormulaStartSymbol) &&
				               !trimmed.EndsWith(Constants.FormulaEndSymbol);

				if (!isCommand && !isStatic)
				{
					return Result.Fail<List<string>>(string.Format("Incorrect syntax near '{0}'", part));
				}
			}

			return Result.Ok(parts);
		}
	}
}