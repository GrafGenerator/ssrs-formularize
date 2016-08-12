using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaBuilder;
using Formularizer.Core.FormulaCompiler;
using Formularizer.Core.HyperlinkAnalyze;
using FunctionalExtensions;

namespace Formularizer.Core
{
	internal class Formularizer
	{
		private static readonly
			List<Func<WorksheetPart, string, DocumentInfo<Result<ICompiledFormula>>, ICompilerStrategy, Result<WorksheetPart>>>
			SheetPatchSteps = new List
				<Func<WorksheetPart, string, DocumentInfo<Result<ICompiledFormula>>, ICompilerStrategy, Result<WorksheetPart>>>
			{
				RemoveHyperlinks,
				PatchCellFormulas
			};

		private readonly SpreadsheetDocument _document;

		public Formularizer(SpreadsheetDocument document)
		{
			_document = document;
		}

		public Result<SpreadsheetDocument> Patch()
		{
			var hypInfo = HyperlinkAnalyzer.Analyze(_document);
			var strategy = new NonFailingCompilerStrategy();

			var compiledDocument = hypInfo
				.OnSuccess(h => Result.Ok(DocumentDefinitionBuilder.Build(h)))
				.OnSuccess(def => new FormulaCompiler.FormulaCompiler(def, strategy).Compile())
				.OnBoth(r => r);

			if (compiledDocument.Failure)
			{
				return Result.Fail<SpreadsheetDocument>(compiledDocument.Error);
			}

			return PatchDocument(_document, compiledDocument.Value, strategy);
		}

		private static Result<SpreadsheetDocument> PatchDocument(SpreadsheetDocument document,
			DocumentInfo<Result<ICompiledFormula>> formulas, ICompilerStrategy strategy)
		{
			var sheets = document.WorkbookPart.WorksheetParts;

			foreach (var sheet in sheets)
			{
				var sheetInfoId = document.WorkbookPart.GetIdOfPart(sheet);

				foreach (var patchStep in SheetPatchSteps)
				{
					var result = patchStep(sheet, sheetInfoId, formulas, strategy);
					if (result.Failure)
					{
						return Result.Fail<SpreadsheetDocument>(result.Error);
					}
				}
			}

			return Result.Ok(document);
		}

		private static Result<WorksheetPart> RemoveHyperlinks(WorksheetPart sheet, string sheetInfoId,
			DocumentInfo<Result<ICompiledFormula>> formulas, ICompilerStrategy strategy)
		{
			var idsSet = new HashSet<string>(
				sheet.HyperlinkRelationships
					.Where(r => r.Uri.AbsoluteUri.StartsWith(Constants.FormulaScheme))
					.Select(r => r.Id)
				);

			var links = sheet.Worksheet.Descendants<Hyperlink>().ToArray();
			foreach (var link in links)
			{
				if (idsSet.Contains(link.Id)) link.Remove();
			}

			var hyperlinks = sheet.Worksheet.Descendants<Hyperlinks>().FirstOrDefault();
			if (hyperlinks != null && !hyperlinks.HasChildren)
			{
				hyperlinks.Remove();
			}

			return Result.Ok(sheet);
		}

		private static Result<WorksheetPart> PatchCellFormulas(WorksheetPart sheet, string sheetInfoId,
			DocumentInfo<Result<ICompiledFormula>> formulas, ICompilerStrategy strategy)
		{
			var sheetInfos = formulas.Sheets.ToDictionary(kv => kv.Value.Id, kv => kv.Value);
			var sheetInfo = sheetInfos.ContainsKey(sheetInfoId) ? sheetInfos[sheetInfoId] : null;
			if (sheetInfo == null)
			{
				return Result.Fail<WorksheetPart>(string.Format("No sheet info found for sheet '{0}'", sheetInfoId));
			}

			var formulasMap = sheetInfo.Formulas.ToDictionary(f => f.Reference, f => f.Value,
				StringComparer.InvariantCultureIgnoreCase);

			foreach (var cell in sheet.Worksheet.Descendants<Cell>())
			{
				if (formulasMap.ContainsKey(cell.CellReference))
				{
					var formulaResult = formulasMap[cell.CellReference];

					if (formulaResult.Success)
					{
						var formulaText = formulaResult.Value.GetFormulaText();

						if (!string.IsNullOrEmpty(formulaText))
							cell.CellFormula = new CellFormula(formulaText);
					}
					else
					{
						if (strategy.OutputFailedFormulas)
						{
							cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);
							var cellText = new Text(formulaResult.Error);
							var inlineString = new InlineString(cellText);
							cell.InlineString = inlineString;
						}
					}
				}
			}

			return Result.Ok(sheet);
		}
	}
}