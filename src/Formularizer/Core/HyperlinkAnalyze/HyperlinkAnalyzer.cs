using System;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Formularizer.Core.Common;
using FunctionalExtensions;

namespace Formularizer.Core.HyperlinkAnalyze
{
	public static class HyperlinkAnalyzer
	{
		public static Result<DocumentInfo<string>> Analyze(SpreadsheetDocument document)
		{
			return CreateDocument(document);
		}


		private static Result<DocumentInfo<string>> CreateDocument(SpreadsheetDocument document)
		{
			var sheetRelationships = document.WorkbookPart.Workbook.Sheets.Cast<Sheet>()
				.ToDictionary(s => s.Id.Value, s => s.Name.Value);

			var sheets = document.WorkbookPart.WorksheetParts;

			var sheetResults = (
				from sheet in sheets
				let id = document.WorkbookPart.GetIdOfPart(sheet)
				let name = sheetRelationships.ContainsKey(id) ? sheetRelationships[id] : ""
				select new
				{
					Name = name,
					Result = CreateSheet(sheet.Worksheet, id, name)
				}
				).ToArray();

			var failedSheet = sheetResults.FirstOrDefault(sr => sr.Result.Failure);
			if (failedSheet != null)
			{
				return
					Result.Fail<DocumentInfo<string>>(string.Format("Sheet info collecting failed: {0}", failedSheet.Result.Error));
			}

			return Result.Ok(DocumentInfo.Create(sheetResults.ToDictionary(sr => sr.Name, sr => sr.Result.Value)));
		}


		private static Result<SheetInfo<string>> CreateSheet(Worksheet sheet, string id, string name)
		{
			Contract.Requires<ArgumentNullException>(sheet != null);

			var relationships = sheet.WorksheetPart.HyperlinkRelationships.ToDictionary(r => r.Id,
				StringComparer.InvariantCultureIgnoreCase);
			var hyperlinks = sheet.Descendants<Hyperlink>();

			var goodLinks = hyperlinks
				.Select(l => CreateFormula(l, relationships[l.Id]))
				.Where(r => r.Success)
				.Select(r => r.Value)
				.ToList();

			return Result.Ok(SheetInfo.Create(id, name, goodLinks));
		}


		private static Result<FormulaInfo<string>> CreateFormula(Hyperlink prototype, ReferenceRelationship relationship)
		{
			Contract.Requires<ArgumentNullException>(prototype != null);
			Contract.Requires<ArgumentNullException>(relationship != null);
			Contract.Requires<ArgumentNullException>(string.Equals(prototype.Id, relationship.Id));

			var reference = prototype.Reference;
			var uri = relationship.Uri;

			if (string.IsNullOrEmpty(uri.AbsoluteUri))
			{
				return Result.Fail<FormulaInfo<string>>("URI is empty");
			}

			if (!Constants.FormulaScheme.Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase))
			{
				return Result.Fail<FormulaInfo<string>>("Not formularizer scheme, skipping");
			}

			var base64UrlEncodedFormula = uri.AbsoluteUri.Replace(Constants.FormulaScheme + "://localhost/?f=", "");
			var base64Encoded = base64UrlEncodedFormula.Replace('_', '/').Replace('-', '+');
			switch (base64UrlEncodedFormula.Length%4)
			{
				case 2:
					base64Encoded += "==";
					break;
				case 3:
					base64Encoded += "=";
					break;
			}
			var bytes = Convert.FromBase64String(base64Encoded);
			var rawFormula = Encoding.UTF8.GetString(bytes);

			return Result.Ok(FormulaInfo.Create(reference, rawFormula));
		}
	}
}