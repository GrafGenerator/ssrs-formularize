using System;
using System.Collections.Generic;
using Formularizer.Core.FormulaDefinition;
using FunctionalExtensions;

namespace Formularizer.Core.FormulaCompiler
{
	internal class FormulaCompilersFactory
	{
		private readonly IDictionary<Type, Func<SheetResolvingContext, IFormulaPartDef, Result<ICompiledFormula>>> _routes;

		public FormulaCompilersFactory()
		{
			_routes = GenerateCompilerRoutes();
		}

		private Dictionary<Type, Func<SheetResolvingContext, IFormulaPartDef, Result<ICompiledFormula>>>
			GenerateCompilerRoutes()
		{
			return new Dictionary<Type, Func<SheetResolvingContext, IFormulaPartDef, Result<ICompiledFormula>>>
			{
				{typeof(StaticTextDef), StaticTextCompiledFormula.Compile},
				{typeof(FormulaDef), ContainerCompiledFormula.Compile},
				{typeof(IdentityDef), IdentityCompiledFormula.Compile},
				{typeof(SelfReferenceCommandDef), SelfReferenceCompiledFormula.Compile},
				{typeof(CellCommandDef), CellCompiledFormula.Compile},
				{typeof(CellsCommandDef), CellsCompiledFormula.Compile},
				{typeof(ColumnCommandDef), ColumnCompiledFormula.Compile}
			};
		}

		public Func<SheetResolvingContext, IFormulaPartDef, Result<ICompiledFormula>> GetCompiler(Type type)
		{
			return _routes.ContainsKey(type) ? _routes[type] : null;
		}
	}
}