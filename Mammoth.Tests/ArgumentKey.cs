using System;

namespace Mammoth.Tests
{
	sealed class ArgumentKey<T> : IEquatable<ArgumentKey<T>>
	{
		public ArgumentKey(string name)
		{
			this.Name = name;
		}

		public string Name { get; }

		public bool Equals(ArgumentKey<T> other)
		{
			return this.Name == other.Name;
		}
	}
}
