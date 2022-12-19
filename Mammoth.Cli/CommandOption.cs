using System;
using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Common.Command
{
	/// <summary>
	/// Command option
	/// </summary>
	public abstract class CommandOptionBase<TValue> : ICommandOption
	{
		private TValue value;

		internal CommandOptionBase() { }

		/// <summary>
		/// Option long name in <c>--name</c> format
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Help string
		/// </summary>
		public string Help { get; set; }
		/// <summary>
		/// Option value or default value if value is not provided
		/// </summary>
		public virtual TValue Value
		{
			get => HasValue ? value : default;
			set
			{
				this.value = value;
				HasValue = true;
			}
		}
		/// <summary>
		/// Option value is provided
		/// </summary>
		public virtual bool HasValue { get; private set; }

		public static implicit operator TValue(CommandOptionBase<TValue> option)
		{
			return option.Value;
		}

		public override string ToString()
		{
			return Value?.ToString();
		}
		object ICommandOption.Value
		{
			get => Value;
			set
			{
				if (value is TValue t)
				{
					this.value = t;
					HasValue = true;
				}
				else
				{
					this.value = default;
					HasValue = false;
				}
			}
		}
	}

	/// <summary>
	/// Command string option
	/// </summary>
	public sealed class CommandOption : CommandOptionBase<string>
	{
	}

	/// <summary>
	/// Command repeatable typed options
	/// </summary>
	/// <typeparam name="TValue">Option value type</typeparam>
	public sealed class CommandOptionList<TValue> : CommandOptionBase<TValue>, ICommandOptionList
		where TValue : struct
	{
		private readonly List<TValue> values = new List<TValue>();

		/// <summary>
		/// Option values
		/// </summary>
		public TValue[] Values => values.ToArray();

		/// <summary>
		/// Adds new value
		/// </summary>
		/// <param name="value">Option value</param>
		public void AddValue(TValue value)
		{
			values.Add(value);
		}

		public static implicit operator TValue[](CommandOptionList<TValue> option)
		{
			return option.Values;
		}

		object[] ICommandOptionList.Values => values.Cast<object>().ToArray();
		void ICommandOptionList.AddValue(object value)
		{
			AddValue((TValue)value);
		}

		/// <summary>
		/// Option values list is not empty
		/// </summary>
		public override bool HasValue => values.Count > 0;
		/// <summary>
		/// Use <see cref="Values"/> property
		/// </summary>
		/// <exception cref="NotSupportedException">Option single value is not supported</exception>
		public override TValue Value
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Command repeatable string options
	/// </summary>
	public sealed class CommandOptionList : CommandOptionBase<string>, ICommandOptionList
	{
		private List<string> values = new List<string>();

		/// <summary>
		/// Option values
		/// </summary>
		public string[] Values => values.ToArray();

		/// <summary>
		/// Adds new value
		/// </summary>
		/// <param name="value">Option value</param>
		public void AddValue(string value)
		{
			values.Add(value);
		}

		public static implicit operator String[](CommandOptionList option)
		{
			return option.Values;
		}

		object[] ICommandOptionList.Values => values.ToArray();
		void ICommandOptionList.AddValue(object value)
		{
			AddValue(value?.ToString());
		}

		/// <summary>
		/// Option values list is not empty
		/// </summary>
		public override bool HasValue => values.Count > 0;
		/// <summary>
		/// Option string values separated by new line
		/// </summary>
		public override string Value
		{
			get => String.Join(Environment.NewLine, values);
			set => values = new List<string>(value
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}
