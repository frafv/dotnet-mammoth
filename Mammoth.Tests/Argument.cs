namespace Mammoth.Tests
{
	abstract class Argument
	{
		public static Argument<T> Arg<T>(ArgumentKey<T> key, T value)
		{
			return new Argument<T>(key, value);
		}
	}

	class Argument<T> : Argument
	{
		internal Argument(ArgumentKey<T> key, T value)
		{
			this.Key = key;
			this.Value = value;
		}

		public ArgumentKey<T> Key { get; }

		public T Value { get; }
	}
}
