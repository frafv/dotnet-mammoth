using System;
using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Results
{
	internal abstract class InternalResult
	{
		internal InternalResult(string[] warnings)
		{
			Warnings = warnings;
		}
		public string[] Warnings { get; }

		public static InternalResult<T[]> Join<T>(IEnumerable<InternalResult<T>> results)
			where T : class
		{
			results = results.ToList();
			return new InternalResult<T[]>(
				results.Select(result => result.Value).ToArray(),
				results.SelectMany(result => result.Warnings).ToArray());
		}
		public static InternalResult<T> Join<T>(params InternalResult[] results)
			where T : class
		{
			return new InternalResult<T>(
				results.OfType<InternalResult<T>>().Last().Value,
				results.SelectMany(result => result.Warnings).ToArray());
		}

		public static InternalResult<R> Map<T, R>(InternalResult<T> first, InternalResult<T> second, Func<T, T, R> function)
			where T : class
			where R : class
		{
			return new InternalResult<R>(
				function(first.Value, second.Value),
				first.Warnings.Concat(second.Warnings).ToArray());
		}

		public static InternalResult<T> Success<T>(T value)
			where T : class
		{
			return new InternalResult<T>(value);
		}
	}
	internal class InternalResult<T> : InternalResult
		where T : class
	{
		internal static readonly InternalResult<T> EMPTY = new InternalResult<T>(null);
		internal InternalResult(T value, params string[] warnings)
			: base(warnings)
		{
			Value = value;
		}
		public T Value { get; }

		public InternalResult<R> Map<R>(Func<T, R> function)
			where R : class
		{
			return new InternalResult<R>(function(Value), Warnings);
		}

		public InternalResult<R> FlatMap<R>(Func<T, InternalResult<R>> function)
			where R : class
		{
			var result = function(Value);
			return new InternalResult<R>(result.Value, Warnings.Concat(result.Warnings).ToArray());
		}

		class Result : IResult<T>
		{
			public Result(T value, params string[] warnings)
			{
				Value = value;
				Warnings = warnings;
			}

			public T Value { get; }
			public string[] Warnings { get; }
		}

		public IResult<T> ToResult()
		{
			return new Result(Value, Warnings);
		}
	}
}

