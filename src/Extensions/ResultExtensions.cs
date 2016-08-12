using System;

namespace FunctionalExtensions
{
	public static class ResultExtensions
	{
		public static Result<T> OnSuccess<T>(this Result result, Func<T> makeResult) where T : class
		{
			if (result.Failure)
				return Result.Fail<T>(result.Error);

			return Result.Ok(makeResult());
		}

		public static Result<T> OnSuccess<T>(this Result result, Func<Result<T>> makeResult) where T : class
		{
			if (result.Failure)
				return Result.Fail<T>(result.Error);

			return makeResult();
		}

		public static Result<TR> OnSuccess<TI, TR>(this Result<TI> result, Func<TI, Result<TR>> makeResult) where TI : class
			where TR : class
		{
			if (result.Failure)
				return Result.Fail<TR>(result.Error);

			return makeResult(result.Value);
		}

		public static Result<TR> OnSuccess<TI, TK, TR>(this Result<TI, TK> result, Func<TI, TK, Result<TR>> makeResult)
			where TI : class
			where TK : class
			where TR : class
		{
			if (result.Failure)
				return Result.Fail<TR>(result.Error);

			return makeResult(result.First, result.Second);
		}

		public static Result<TR1, TR2> OnSuccess<TI, TR1, TR2>(this Result<TI> result, Func<TI, Result<TR1, TR2>> makeResult)
			where TI : class
			where TR1 : class
			where TR2 : class
		{
			if (result.Failure)
				return Result.Fail<TR1, TR2>(result.Error);

			return makeResult(result.Value);
		}


		public static Result OnFailure(this Result result, Action action)
		{
			if (result.Failure)
			{
				action();
			}

			return result;
		}

		public static Result<T> OnFailure<T>(this Result<T> result, Action action) where T : class
		{
			if (result.Failure)
			{
				action();
			}

			return result;
		}

		public static Result<TI, TK> OnFailure<TI, TK>(this Result<TI, TK> result, Action action)
			where TI : class
			where TK : class
		{
			if (result.Failure)
			{
				action();
			}

			return result;
		}


		public static Result OnBoth(this Result result, Action<Result> action)
		{
			action(result);

			return result;
		}

		public static T OnBoth<T>(this Result<T> result, Func<Result<T>, T> makeResult) where T : class
		{
			return makeResult(result);
		}

		public static TR OnBoth<TI, TR>(this Result<TI> result, Func<Result<TI>, TR> makeResult) where TI : class
		{
			return makeResult(result);
		}

		public static TR OnBoth<TI, TK, TR>(this Result<TI, TK> result, Func<Result<TI, TK>, TR> makeResult)
			where TI : class
			where TK : class
		{
			return makeResult(result);
		}


		public static Result<TI, TK> Attach<TI, TK>(this Result<TI> result, TK toAttach) where TI : class where TK : class
		{
			if (result.Failure)
				return Result.Fail<TI, TK>(result.Error);

			return Result.Ok(result.Value, toAttach);
		}
	}
}