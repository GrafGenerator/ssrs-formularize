using System;
using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class SheetResolvingContext
	{
		private SheetInfo<IFormulaPartDef> _currentSheet;
		private readonly bool _skipContractVerification;


		public SheetResolvingContext(IEnumerable<SheetInfo<IFormulaPartDef>> sheets, FormulaCompilersFactory compilersFactory)
		{
			CompilersFactory = compilersFactory;
			Sheets = sheets.ToArray();
			CurrentSheet = null;
			CompiledFormulas = new List<ICompiledFormula>();
			_skipContractVerification = true;
		}

		private SheetResolvingContext(SheetResolvingContext other, SheetInfo<IFormulaPartDef> current)
		{
			CompilersFactory = other.CompilersFactory;
			Sheets = other.Sheets.Concat(new[] {other.CurrentSheet})
				.Where(s => s != null)
				.Except(new[] {current})
				.ToArray();
			CurrentSheet = current;
			CompiledFormulas = new List<ICompiledFormula>();
			_skipContractVerification = false;
		}

		private SheetResolvingContext(SheetResolvingContext other)
		{
			CompilersFactory = other.CompilersFactory;
			Sheets = other.Sheets;
			CurrentSheet = other.CurrentSheet;
			CompiledFormulas = new List<ICompiledFormula>();
			_skipContractVerification = other._skipContractVerification;
		}

		public FormulaCompilersFactory CompilersFactory { get; set; }
		public SheetInfo<IFormulaPartDef>[] Sheets { get; private set; }
		public IList<ICompiledFormula> CompiledFormulas { get; private set; }

		public SheetInfo<IFormulaPartDef> CurrentSheet
		{
			get
			{
				Contract.Requires<InvalidOperationException>(_skipContractVerification || _currentSheet != null);
				return _currentSheet;
			}
			private set { _currentSheet = value; }
		}


		public Result<SheetContext> GetSheetContext(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return Result.Ok(SheetContext.Create(CurrentSheet, true));
			}

			var sheet = Sheets.FirstOrDefault(s => name.Equals(s.Name, StringComparison.InvariantCultureIgnoreCase));
			if (sheet == null)
			{
				return Result.Fail<SheetContext>(string.Format("Can't find sheet '{0}'", name));
			}

			return
				Result.Ok(SheetContext.Create(sheet,
					string.Equals(CurrentSheet.Name, sheet.Name, StringComparison.InvariantCultureIgnoreCase)));
		}


		public void AppendFormula(ICompiledFormula formula)
		{
			CompiledFormulas.Add(formula);
		}

		public Result<ICompiledFormula> CompileFormula(IFormulaPartDef definition)
		{
			var type = definition.GetType();
			var compiler = CompilersFactory.GetCompiler(type);
			if (compiler == null)
			{
				return Result.Fail<ICompiledFormula>(string.Format("Cannot find compiler for formula definition '{0}'", type.Name));
			}

			return compiler(this, definition);
		}

		public Result<string> CompileFilter(SelectorCommandDef def)
		{
			Contract.Requires<ArgumentNullException>(def != null);

			var filter = def.Filter;

			if (def.UseOwnId)
			{
				var idPart = CompiledFormulas.FirstOrDefault(f => f is IdentityCompiledFormula) as IdentityCompiledFormula;

				if (idPart == null)
				{
					return Result.Fail<string>("Internal error, no identity in formula, using own identity");
				}

				filter = filter.Replace(Constants.OwnIdPlaceholder, idPart.Id);
			}

			return Result.Ok(filter);
		}


		public SheetResolvingContext EnterSheet(SheetInfo<IFormulaPartDef> sheet)
		{
			return new SheetResolvingContext(this, sheet);
		}

		public SheetResolvingContext EnterFormula()
		{
			return new SheetResolvingContext(this);
		}
	}
}