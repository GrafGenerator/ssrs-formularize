using System;
using System.Text.RegularExpressions;
using Formularizer.Core.Common;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
	public class CellCommandDef : SelectorCommandDef
	{
		private static readonly Regex CommandRegex =
			new Regex(@"cell\s*'(?<filter>[\w\s,.*+-]+)'\s*(?<scope>[\w]+)\s*('(?<sheet>[\w\s(),.*+-]+)')*",
				RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		protected CellCommandDef(string sheetName, bool useOwnId, string filter, SelectorScope scope, string reference)
			: base(sheetName, useOwnId, filter, scope, reference)
		{
		}

		public static Result<IFormulaPartDef> Create(string rawText, string reference)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(rawText));

			var match = CommandRegex.Match(rawText);
			if (!match.Success)
			{
				return Result.Fail<IFormulaPartDef>(string.Format("Incorrect syntax of 'cell' command: '{0}'", rawText));
			}

			var sheetName = match.Groups["sheet"].Captures.Count > 0
				? match.Groups["sheet"].Captures[0].Value
				: "";

			var filter = match.Groups["filter"].Captures.Count > 0
				? match.Groups["filter"].Captures[0].Value
				: null;

			if (filter == null)
			{
				return Result.Fail<IFormulaPartDef>(string.Format("Filter required: '{0}'", rawText));
			}

			var scopeString = match.Groups["scope"].Captures.Count > 0
				? match.Groups["scope"].Captures[0].Value
				: null;

			if (scopeString == null)
			{
				return Result.Fail<IFormulaPartDef>(string.Format("Search scope required: '{0}'", rawText));
			}

			SelectorScope scope;
			try
			{
				scope = (SelectorScope) Enum.Parse(typeof(SelectorScope), scopeString, true);
			}
			catch
			{
				return Result.Fail<IFormulaPartDef>(string.Format("Search scope '{0}' not allowed.", scopeString));
			}

			var useOwnId = filter.IndexOf(Constants.OwnIdPlaceholder, StringComparison.Ordinal) > -1;

			return Result.Ok((IFormulaPartDef) new CellCommandDef(sheetName, useOwnId, filter, scope, reference));
		}
	}
}