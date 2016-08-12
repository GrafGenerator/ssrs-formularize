namespace Formularizer.Core.Common
{
	internal static class Utility
	{
		public static string[] SplitReference(string reference)
		{
			var idx = 0;
			for (; idx < reference.Length && !char.IsDigit(reference[idx]); idx++)
			{
			}

			return new[]
			{
				reference.Substring(0, idx),
				reference.Substring(idx)
			};
		}
	}
}