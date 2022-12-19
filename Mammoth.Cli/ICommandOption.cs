namespace Mammoth.Common.Command
{
	/// <summary>
	/// Command option
	/// </summary>
	public interface ICommandOption
	{
		/// <summary>
		/// Option long name in <c>--name</c> format
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Help string
		/// </summary>
		string Help { get; }
		/// <summary>
		/// Option value
		/// </summary>
		object Value { get; set; }
		/// <summary>
		/// Option value is provided
		/// </summary>
		bool HasValue { get; }
	}

	/// <summary>
	/// Command repeatable options
	/// </summary>
	public interface ICommandOptionList : ICommandOption
	{
		/// <summary>
		/// Option values
		/// </summary>
		object[] Values { get; }
		/// <summary>
		/// Adds new value
		/// </summary>
		/// <param name="value">Option value</param>
		void AddValue(object value);
	}
}
