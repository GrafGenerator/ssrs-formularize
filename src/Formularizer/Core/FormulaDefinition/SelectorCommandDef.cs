namespace Formularizer.Core.FormulaDefinition
{
	public class SelectorCommandDef : ICommandPartDef
	{
		protected SelectorCommandDef(string sheetName, bool useOwnId, string filter, SelectorScope scope, string reference)
		{
			Reference = reference;
			SheetName = sheetName;
			UseOwnId = useOwnId;
			Filter = filter;
			Scope = scope;
		}

		public bool UseOwnId { get; private set; }
		public string Filter { get; private set; }
		public SelectorScope Scope { get; private set; }
		public string Reference { get; private set; }
		public string SheetName { get; private set; }
	}

	public enum SelectorScope
	{
		Column,
		Row,
		All
	}
}