namespace Formularizer.Core.Common
{
	public class FormulaInfo
	{
		public static FormulaInfo<T> Create<T>(string reference, T value)
		{
			return new FormulaInfo<T>(reference, value);
		}
	}

	public class FormulaInfo<T> : FormulaInfo
	{
		protected internal FormulaInfo(string reference, T value)
		{
			Reference = reference;
			Value = value;
		}

		public string Reference { get; private set; }
		public T Value { get; private set; }
	}
}