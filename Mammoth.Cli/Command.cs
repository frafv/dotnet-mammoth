using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mammoth.Common.Command
{
	/// <summary>
	/// Application command
	/// </summary>
	public class Command
	{
		/// <summary>
		/// Application name
		/// </summary>
		public string Application { get; }
		/// <summary>
		/// Console supports colors
		/// </summary>
		public bool ConsoleColors { get; set; } = true;
		/// <summary>
		/// Command parameters
		/// </summary>
		public ICommandParameter[] Parameters { get; set; }
		/// <summary>
		/// Command options
		/// </summary>
		public ICommandOption[] Options { get; set; }

		/// <summary>
		/// Application command
		/// </summary>
		/// <param name="application">Application default name if no permissions to resolve it</param>
		public Command(string application = null)
		{
			try
			{
				string domain = AppDomain.CurrentDomain.FriendlyName;
				if (!String.IsNullOrEmpty(domain))
					application = Path.GetFileNameWithoutExtension(domain);
			}
			catch { }
			Application = application;
		}

		private IEnumerable<string> HelpCommand()
		{
			foreach (var parameter in Parameters)
			{
				yield return parameter.Required ? $"<{parameter.Name}>" : $"[{parameter.Name}]";
			}
		}

		private IEnumerable<string> HelpParameters()
		{
			int indent = Parameters.Max(x => x.Name.Length);
			indent += 5;
			foreach (var parameter in Parameters)
			{
				string space = new string(' ', indent - parameter.Name.Length);
				string[] help = parameter.Help.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				yield return $"{parameter.Name}{space}{help[0]}";
				space = new string(' ', indent);
				foreach (string line in help.Skip(1))
					yield return $"{space}{line}";
			}
		}

		private string HelpOptionName(ICommandOption option)
		{
			string name = $"--{option.Name}";
			return name;
		}

		private IEnumerable<string> HelpOptions()
		{
			int indent = Options.Max(x => HelpOptionName(x).Length);
			indent += 4;
			foreach (var option in Options)
			{
				string name = HelpOptionName(option);
				string space = new string(' ', indent - name.Length);
				string[] lines = option.Help.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				yield return $"   {name}{space}{lines.First()}";
				space = new string(' ', indent + 3);
				foreach (string line in lines.Skip(1))
					yield return space + line;
			}
		}

		private class ConsoleColorContext : IDisposable
		{
			public static IDisposable Create(ConsoleColor color)
			{
				try
				{
					Console.ForegroundColor = color;
					return new ConsoleColorContext();
				}
				catch
				{
					return null;
				}
			}

			public void Dispose()
			{
				try
				{
					Console.ResetColor();
				}
				catch { }
			}
		}

		private IDisposable ConsoleWrite(ConsoleColor color)
		{
			if (!ConsoleColors) return null;
			var result = ConsoleColorContext.Create(color);
			if (result == null) ConsoleColors = false;
			return result;
		}

		private bool Help()
		{
			string cmd = String.Join(" ", HelpCommand());
			using (ConsoleWrite(ConsoleColor.White))
			{
				Console.WriteLine($"Usage: {Application} {cmd} [options]");
				Console.WriteLine();
				foreach (string parameter in HelpParameters())
					Console.WriteLine(parameter);
				Console.WriteLine();
				Console.WriteLine("Options:");
				foreach (string option in HelpOptions())
					Console.WriteLine(option);
			}
			return false;
		}

		/// <summary>
		/// Output error message (in red if possible)
		/// </summary>
		/// <param name="message">Error message</param>
		public bool Error(string message)
		{
			using (ConsoleWrite(ConsoleColor.Red))
			{
				Console.Error.WriteLine(message);
			}
			Console.WriteLine();
			return false;
		}

		/// <summary>
		/// Output warning message (in yellow if possible)
		/// </summary>
		/// <param name="message">Warning message</param>
		public bool Warning(string message)
		{
			using (ConsoleWrite(ConsoleColor.Yellow))
			{
				Console.Error.WriteLine(message);
			}
			return true;
		}

		/// <summary>
		/// Output information message (in white if possible)
		/// </summary>
		/// <param name="message">Information message</param>
		public bool Info(string message)
		{
			using (ConsoleWrite(ConsoleColor.White))
			{
				Console.Error.WriteLine(message);
			}
			Console.WriteLine();
			return true;
		}

		private static bool HasHelp(string[] args)
		{
			if (args == null || args.Length == 0)
				return true;
			return args.Any(x => x.Equals("-h", StringComparison.Ordinal) ||
				x.Equals("--help", StringComparison.Ordinal) ||
				x.Equals("-?", StringComparison.Ordinal));
		}

		private static bool IsOption(string arg)
		{
			return arg.StartsWith("-", StringComparison.Ordinal);
		}

		private static bool OptionNameEquals(string arg, ICommandOption option)
		{
			return arg.Equals($"--{option.Name}", StringComparison.Ordinal);
		}

		private bool ParseOption(string[] args, ref int k)
		{
			string arg = args[k];
			var option = Options.FirstOrDefault(x => OptionNameEquals(arg, x));
			if (option == null)
				return Warning($"'{arg}' is skipped as unknown");
			if (k + 1 >= args.Length || IsOption(args[k + 1]))
				return Error($"'{arg}' expects a value");
			k++;
			object value = ParseValue(arg, args[k], option.GetType());
			if (value == null) return false;
			if (option is ICommandOptionList list)
				list.AddValue(value);
			else if (!option.HasValue)
				option.Value = value;
			else
				return Error($"'{arg}' is duplicated");
			return true;
		}

		private object ParseValue(string arg, string value, Type type)
		{
			return value;
		}

		private bool ParseParameter(string arg, ICommandParameter parameter)
		{
			if (parameter.Value == null)
				parameter.Value = arg;
			else
				return false;
			return true;
		}

		/// <summary>
		/// Parses command arguments line
		/// </summary>
		/// <param name="args">Command arguments</param>
		/// <returns>Command arguments are provided</returns>
		public bool TryParse(string[] args)
		{
			if (HasHelp(args))
				return Help();

			for (int k = 0, p = 0; k < args.Length; k++)
			{
				string arg = args[k];
				if (IsOption(arg))
				{
					if (!ParseOption(args, ref k))
						return Help();
				}
				else
				{
					if (p >= Parameters.Length)
						return Help();
					var parameter = Parameters[p];
					p++;
					if (!ParseParameter(arg, parameter))
						return Help();
				}
			}

			var required = Parameters.FirstOrDefault(x => x.Required && x.Value == null);
			if (required != null)
			{
				Error($"{required.Name} argument is required");
				return Help();
			}

			return true;
		}
	}
}
