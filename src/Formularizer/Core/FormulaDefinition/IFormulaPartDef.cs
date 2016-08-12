using System.Collections.Generic;

namespace Formularizer.Core.FormulaDefinition
{
	public interface IFormulaPartDef
	{
		string Reference { get; }
	}

	public interface IFormulaPartContainerDef : IFormulaPartDef
	{
		IEnumerable<IFormulaPartDef> Parts { get; }
	}

	public interface ICommandPartDef : IFormulaPartDef
	{
		string SheetName { get; }
	}
}