using System;
using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaBuilder
{
	internal class FormulaSignatureMatchingFactory
	{
		private readonly Dictionary<string, Func<string, string, Result<IFormulaPartDef>>> _commandMatchers;

		public FormulaSignatureMatchingFactory()
		{
			_commandMatchers = GenerateCommandsMatchers();
		}

		private Dictionary<string, Func<string, string, Result<IFormulaPartDef>>> GenerateCommandsMatchers()
		{
			return new Dictionary<string, Func<string, string, Result<IFormulaPartDef>>>
			{
				{Constants.CellsCommand, CellsCommandDef.Create},
				{Constants.CellCommand, CellCommandDef.Create},
				{Constants.ColumnCommand, ColumnCommandDef.Create},
				{Constants.ThisCommand, SelfReferenceCommandDef.Create},
				{Constants.IdentityCommand, IdentityDef.Create}
			};
		}

		public Result<IFormulaPartDef> MatchDef(string rawText, string reference)
		{
			var preparedText = rawText.Trim();
			var isStaticPart = !preparedText.StartsWith(Constants.FormulaStartSymbol) &&
			                   !preparedText.EndsWith(Constants.FormulaEndSymbol);

			Result<IFormulaPartDef> result;

			if (isStaticPart)
			{
				result = StaticTextDef.Create(rawText, reference);
			}
			else
			{
				var commandText = preparedText.Trim()
					.TrimStart(Constants.FormulaStartChar)
					.TrimEnd(Constants.FormulaEndChar)
					.Trim();

				var knownCommandName = _commandMatchers
					.Where(m => commandText.StartsWith(m.Key, StringComparison.InvariantCultureIgnoreCase))
					.Select(m => m.Key)
					.FirstOrDefault();

				result = knownCommandName == null
					? Result.Fail<IFormulaPartDef>(string.Format("Unknown commands '{0}'", commandText))
					: _commandMatchers[knownCommandName](commandText, reference);
			}

			return result;
		}
	}
}