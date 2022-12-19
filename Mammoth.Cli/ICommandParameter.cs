namespace Mammoth.Common.Command
{
	/// <summary>
	/// Command string parameter
	/// </summary>
	public interface ICommandParameter
	{
		/// <summary>
		/// Help string
		/// </summary>
		string Help { get; set; }
		/// <summary>
		/// Parameter name
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Parameter is required
		/// </summary>
		bool Required { get; set; }
		/// <summary>
		/// Parameter string value
		/// </summary>
		string Value { get; set; }
	}
}