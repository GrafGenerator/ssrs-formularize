using System;
using System.Text.RegularExpressions;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
	public class SelfReferenceCommandDef : ICommandPartDef
	{
		private static readonly Regex CommandRegex = new Regex(@"this\s*'(?<sheet>[\w\s*+-]+)'|this",
			RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		private SelfReferenceCommandDef(string sheetName, string reference)
		{
			Reference = reference;
			SheetName = sheetName;
		}

		public string SheetName { get; private set; }
		public string Reference { get; private set; }

		public static Result<IFormulaPartDef> Create(string rawText, string reference)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(rawText));

			var match = CommandRegex.Match(rawText);
			if (!match.Success)
			{
				return Result.Fail<IFormulaPartDef>(string.Format("Incorrect syntax of 'this' command: '{0}'", rawText));
			}

			var sheetName = match.Groups["sheet"].Captures.Count > 0
				? match.Groups["sheet"].Captures[0].Value
				: "";

			return Result.Ok((IFormulaPartDef) new SelfReferenceCommandDef(sheetName, reference));
		}
	}
}