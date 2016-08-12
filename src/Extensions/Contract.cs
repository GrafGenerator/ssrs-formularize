using System;
using System.Diagnostics;

namespace FunctionalExtensions
{
	public class Contract
	{
		public static void Requires<TException>(bool predicate, string message = "")
			where TException : Exception, new()
		{
			if (predicate) return;

			Debug.WriteLine(message);
			throw new TException();
		}
	}
}