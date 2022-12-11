using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Tests
{
	class Arguments
	{
		// TODO: make sure all arguments are used

		readonly Argument[] arguments;

		public Arguments(Argument[] arguments)
		{
			this.arguments = arguments;
		}

		public T Get<T>(ArgumentKey<T> key, T defaultValue)
		{
			return GetAll(key).DefaultIfEmpty(defaultValue).Single();
		}

		IEnumerable<T> GetAll<T>(ArgumentKey<T> key)
		{
			return arguments
				.OfType<Argument<T>>()
				.Where(argument => argument.Key.Equals(key))
				.Select(argument => argument.Value);
		}
	}

	class ArgumentValues
	{
		readonly object[] arguments;

		public ArgumentValues(object[] arguments)
		{
			this.arguments = arguments;
		}

		public T Get<T>()
		{
			return arguments.OfType<T>().Single();
		}

		public T Get<T>(T defaultValue)
		{
			return arguments.OfType<T>().DefaultIfEmpty(defaultValue).Single();
		}
	}
}
