using System;
using System.Text.RegularExpressions;
using Formularizer.Core.Common;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaDefinition
{
    public class ColumnCommandDef : SelectorCommandDef
    {
        protected ColumnCommandDef(string sheetName, bool useOwnId, string filter, SelectorScope scope, string reference)
            : base(sheetName, useOwnId, filter, scope, reference)
        {
        }

        private static readonly Regex CommandRegex = new Regex(@"column\s*'(?<filter>[\w\s,.*+-]+)'\s*('(?<sheet>[\w\s(),.*+-]+)')*",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static Result<IFormulaPartDef> Create(string rawText, string reference)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(rawText));

            var match = CommandRegex.Match(rawText);
            if (!match.Success)
            {
                return Result.Fail<IFormulaPartDef>(string.Format("Incorrect syntax of 'column' command: '{0}'", rawText));
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

            var useOwnId = filter.IndexOf(Constants.OwnIdPlaceholder, StringComparison.Ordinal) > -1;

            return Result.Ok((IFormulaPartDef)new ColumnCommandDef(sheetName, useOwnId, filter, SelectorScope.Column, reference));
        }
    }
}