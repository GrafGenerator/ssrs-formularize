using System.Linq;
using Formularizer.Core.Common;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	public interface ICompilerStrategy
	{
		bool OutputFailedFormulas { get; }
		Result CheckCompilation(DocumentInfo<Result<ICompiledFormula>> documentInfo);
	}

	public class FailOnFailedFormulasCompilerStrategy : ICompilerStrategy
	{
		public Result CheckCompilation(DocumentInfo<Result<ICompiledFormula>> documentInfo)
		{
			var failedFormula =
				documentInfo.Sheets.SelectMany(kvSheet => kvSheet.Value.Formulas).FirstOrDefault(f => f.Value.Failure);

			return failedFormula != null ? Result.Fail(failedFormula.Value.Error) : Result.Ok();
		}

		public bool OutputFailedFormulas
		{
			get { return true; }
		}
	}

	public class NonFailingCompilerStrategy : ICompilerStrategy
	{
		public Result CheckCompilation(DocumentInfo<Result<ICompiledFormula>> documentInfo)
		{
			return Result.Ok();
		}

		public bool OutputFailedFormulas
		{
			get { return true; }
		}
	}
}