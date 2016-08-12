using System;
using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.FormulaCompiler;
using FunctionalExtensions;

namespace Formularizer.Core.Common
{
	internal static class DocumentInfoExtensions
	{
		public static DocumentInfo<TR> Traverse<TI, TR>(this DocumentInfo<TI> info,
			Func<FormulaInfo<TI>, Result<TR>> makeFormulaValue, bool removeFailed = true) where TR : class
		{
			var newSheets =
				from kv in info.Sheets
				let sheetInfo = SheetInfo.Create(kv.Value.Id, kv.Key,
					(
						from formula in kv.Value.Formulas
						let formulaValue = makeFormulaValue(formula)
						where !removeFailed || formulaValue.Success
						let formulaInfo = FormulaInfo.Create(formula.Reference, formulaValue.Value)
						select formulaInfo
						).ToList())
				select new KeyValuePair<string, SheetInfo<TR>>(kv.Key, sheetInfo);

			return DocumentInfo.Create(newSheets.ToDictionary(kv => kv.Key, kv => kv.Value));
		}

		public static DocumentInfo<Result<TR>> TraverseResult<TI, TR>(this DocumentInfo<TI> info,
			Func<SheetResolvingContext, FormulaInfo<TI>, Result<TR>> makeFormulaValue,
			Func<SheetInfo<TI>, SheetResolvingContext> transformContext, bool removeFailed = true) where TR : class
		{
			var newSheets =
				from kv in info.Sheets
				let sheetContext = transformContext(kv.Value)
				let sheetInfo = SheetInfo.Create(kv.Value.Id, kv.Key,
					(
						from formula in kv.Value.Formulas
						let formulaValue = makeFormulaValue(sheetContext, formula)
						where !removeFailed || formulaValue.Success
						let formulaInfo = FormulaInfo.Create(formula.Reference, formulaValue)
						select formulaInfo
						).ToList())
				select new KeyValuePair<string, SheetInfo<Result<TR>>>(kv.Key, sheetInfo);

			return DocumentInfo.Create(newSheets.ToDictionary(kv => kv.Key, kv => kv.Value));
		}
	}
}