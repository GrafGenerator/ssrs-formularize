using System.Collections.Generic;

namespace Formularizer.Core.Common
{
	public class SheetInfo
	{
		public static SheetInfo<T> Create<T>(string id, string name, IList<FormulaInfo<T>> formulas)
		{
			return new SheetInfo<T>(id, name, formulas);
		}
	}

	public class SheetInfo<T> : SheetInfo
	{
		protected internal SheetInfo(string id, string name, IList<FormulaInfo<T>> formulas)
		{
			Id = id;
			Name = name;
			Formulas = formulas;
		}

		public IList<FormulaInfo<T>> Formulas { get; private set; }
		public string Name { get; private set; }
		public string Id { get; private set; }
	}
}