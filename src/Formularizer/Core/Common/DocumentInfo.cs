using System.Collections.Generic;

namespace Formularizer.Core.Common
{
	public class DocumentInfo
	{
		public static DocumentInfo<T> Create<T>(IDictionary<string, SheetInfo<T>> sheets)
		{
			return new DocumentInfo<T>(sheets);
		}
	}

	public class DocumentInfo<T> : DocumentInfo
	{
		protected internal DocumentInfo(IDictionary<string, SheetInfo<T>> sheets)
		{
			Sheets = sheets;
		}

		public IDictionary<string, SheetInfo<T>> Sheets { get; private set; }
	}
}