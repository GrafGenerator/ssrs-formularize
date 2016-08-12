using System;
using DocumentFormat.OpenXml.Packaging;

namespace FormularizerTests
{
	internal class FileRunHelper : IDisposable
	{
		private readonly SpreadsheetDocument _document;

		public FileRunHelper(string path)
		{
			_document = SpreadsheetDocument.Open(path, true);
		}

		public SpreadsheetDocument Document
		{
			get { return _document; }
		}

		public void Dispose()
		{
			_document.Close();
		}
	}
}