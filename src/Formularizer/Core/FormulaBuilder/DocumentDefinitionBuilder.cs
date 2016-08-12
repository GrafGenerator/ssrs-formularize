using Formularizer.Core.Common;
using Formularizer.Core.FormulaDefinition;

namespace Formularizer.Core.FormulaBuilder
{
	internal class DocumentDefinitionBuilder
	{
		public static DocumentInfo<IFormulaPartDef> Build(DocumentInfo<string> documentInfo)
		{
			var defBuilder = new FormulaDefinitionBuilder();
			return documentInfo.Traverse(defBuilder.BuildDef);
		}
	}
}