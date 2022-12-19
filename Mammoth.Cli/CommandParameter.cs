using System;

namespace Mammoth.Common.Command
{
	/// <summary>
	/// Command parameter
	/// </summary>
	public abstract class CommandParameterBase : ICommandParameter
	{
		internal CommandParameterBase() { }

		/// <summary>
		/// Parameter name
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Parameter is required
		/// </summary>
		public bool Required { get; set; }
		/// <summary>
		/// Help string
		/// </summary>
		public string Help { get; set; }
		/// <summary>
		/// Parameter string value
		/// </summary>
		public virtual string Value { get; set; }

		public static implicit operator String(CommandParameterBase parameter)
		{
			return parameter.Value;
		}

		public override string ToString()
		{
			return Value;
		}
	}

	/// <summary>
	/// Command string parameter
	/// </summary>
	public sealed class CommandParameter : CommandParameterBase
	{
	}
}
