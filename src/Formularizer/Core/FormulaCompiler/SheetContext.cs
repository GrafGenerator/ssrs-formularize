using System;
using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class SheetContext
	{
		private string _contextualPrefix;

		private SheetContext(SheetInfo<IFormulaPartDef> sheetInfo, bool isCurrent)
		{
			Contract.Requires<ArgumentNullException>(sheetInfo != null);
			Sheet = sheetInfo;
			IsCurrent = isCurrent;
		}

		public SheetInfo<IFormulaPartDef> Sheet { get; private set; }
		public bool IsCurrent { get; set; }

		public string ContextualPrefix
		{
			get
			{
				if (_contextualPrefix != null) return _contextualPrefix;

				var sheetNameHasWhitespaces = Sheet.Name.ToCharArray().Any(char.IsWhiteSpace);
				_contextualPrefix = string.Format(IsCurrent ? "" : sheetNameHasWhitespaces ? "'{0}'!" : "{0}!", Sheet.Name);

				return _contextualPrefix;
			}
		}

		public IEnumerable<FormulaInfo<IFormulaPartDef>> GetById(string id)
		{
			return Sheet.Formulas.Where(fi => fi.Value is FormulaDef)
				.Where(fi => (fi.Value as FormulaDef).Parts.OfType<IdentityDef>()
					.Any(i => i.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)));
		}

		public IEnumerable<FormulaInfo<IFormulaPartDef>> GetByIdInScope(string reference, string id, SelectorScope scope)
		{
			if (scope == SelectorScope.All)
				return GetById(id);

			var comparer = new ScopedReferenceComparer(scope);

			return Sheet.Formulas.Where(fi => fi.Value is FormulaDef)
				.Where(fi =>
					(fi.Value as FormulaDef).Parts
						.OfType<IdentityDef>()
						.Any(i => i.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)))
				.Where(fi => comparer.Equals(reference,
					(fi.Value as FormulaDef).Parts
						.OfType<IdentityDef>()
						.First().Reference));
		}

		public static SheetContext Create(SheetInfo<IFormulaPartDef> sheetInfo, bool isCurrent)
		{
			return new SheetContext(sheetInfo, isCurrent);
		}

		private class ScopedReferenceComparer : EqualityComparer<string>
		{
			private readonly SelectorScope _scope;

			public ScopedReferenceComparer(SelectorScope scope)
			{
				_scope = scope;
			}

			public override bool Equals(string x, string y)
			{
				var xParts = Utility.SplitReference(x);
				var yParts = Utility.SplitReference(y);

				switch (_scope)
				{
					case SelectorScope.Column:
						return string.Equals(xParts[0], yParts[0], StringComparison.InvariantCultureIgnoreCase);
					case SelectorScope.Row:
						return string.Equals(xParts[1], yParts[1], StringComparison.InvariantCultureIgnoreCase);
					default:
						return false;
				}
			}

			public override int GetHashCode(string obj)
			{
				throw new NotImplementedException();
			}
		}
	}
}