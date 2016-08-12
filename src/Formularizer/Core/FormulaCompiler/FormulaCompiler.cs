using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	public class FormulaCompiler
	{
		private readonly FormulaCompilersFactory _compilersFactory;
		private readonly DocumentInfo<IFormulaPartDef> _documentInfo;
		private readonly ICompilerStrategy _strategy;

		public FormulaCompiler(DocumentInfo<IFormulaPartDef> documentInfo, ICompilerStrategy strategy)
		{
			_documentInfo = documentInfo;
			_strategy = strategy;
			_compilersFactory = new FormulaCompilersFactory();
		}

		public Result<DocumentInfo<Result<ICompiledFormula>>> Compile()
		{
			var context = new SheetResolvingContext(_documentInfo.Sheets.Values, _compilersFactory);

			var compiledDocumentInfo = _documentInfo.TraverseResult(
				(ctx, f) => ctx.CompileFormula(f.Value), context.EnterSheet,
				false);

			var verificationResult = _strategy.CheckCompilation(compiledDocumentInfo);

			return verificationResult.Success
				? Result.Ok(compiledDocumentInfo)
				: Result.Fail<DocumentInfo<Result<ICompiledFormula>>>(verificationResult.Error);
		}
	}
}