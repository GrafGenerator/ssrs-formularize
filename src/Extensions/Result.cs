using System;

namespace FunctionalExtensions
{
	public class Result
	{
		protected Result(bool success, string error)
		{
			Contract.Requires<ArgumentException>((success && string.IsNullOrEmpty(error)) ||
			                                     (!success && !string.IsNullOrEmpty(error)));

			Success = success;
			Error = error;
		}

		public bool Success { get; private set; }
		public string Error { get; private set; }

		public bool Failure
		{
			get { return !Success; }
		}

		public static Result Fail(string message)
		{
			return new Result(false, message);
		}

		public static Result<T> Fail<T>(string message) where T : class
		{
			return new Result<T>(default(T), false, message);
		}

		public static Result<T1, T2> Fail<T1, T2>(string message) where T1 : class where T2 : class
		{
			return new Result<T1, T2>(default(T1), default(T2), false, message);
		}

		public static Result Ok()
		{
			return new Result(true, string.Empty);
		}

		public static Result<T> Ok<T>(T value) where T : class
		{
			return new Result<T>(value, true, string.Empty);
		}

		public static Result<T1, T2> Ok<T1, T2>(T1 first, T2 second) where T1 : class where T2 : class
		{
			return new Result<T1, T2>(first, second, true, string.Empty);
		}

		public static Result Combine(params Result[] results)
		{
			foreach (var result in results)
			{
				if (result.Failure)
					return result;
			}

			return Ok();
		}
	}


	public class Result<T> : Result
		where T : class
	{
		private T _value;

		protected internal Result(T value, bool success, string error)
			: base(success, error)
		{
			Contract.Requires<InvalidOperationException>(value != null || !success);

			Value = value;
		}

		public T Value
		{
			get
			{
				Contract.Requires<InvalidOperationException>(Success);

				return _value;
			}
			private set { _value = value; }
		}
	}

	public class Result<TI, TK> : Result
		where TI : class
		where TK : class
	{
		private TI _first;
		private TK _second;

		protected internal Result(TI first, TK second, bool success, string error)
			: base(success, error)
		{
			Contract.Requires<InvalidOperationException>(first != null && second != null || !success);

			First = first;
			Second = second;
		}

		public TI First
		{
			get
			{
				Contract.Requires<InvalidOperationException>(Success);

				return _first;
			}
			private set { _first = value; }
		}

		public TK Second
		{
			get
			{
				Contract.Requires<InvalidOperationException>(Success);

				return _second;
			}
			private set { _second = value; }
		}
	}
}