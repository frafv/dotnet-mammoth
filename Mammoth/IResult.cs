namespace Mammoth
{
	/// <summary>
	/// Represents the result of a conversion.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IResult<T>
	{
		/// <summary>
		/// The generated text.
		/// </summary>
		T Value { get; }
		/// <summary>
		/// Any warnings generated during the conversion.
		/// </summary>
		string[] Warnings { get; }
	}
}

