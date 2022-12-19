using System;
using System.Collections.Generic;
using System.IO;
using Mammoth.Common.Command;

namespace Mammoth.Cli
{
	internal class Program
	{
		static readonly CommandParameter docPath = new CommandParameter
		{
			Name = "docx-path",
			Required = true,
			Help = "Path to the .docx file to convert."
		};
		static readonly CommandParameter outputPath = new CommandParameter
		{
			Name = "output-path",
			Help = "Output path for the generated document. Images will be stored inline in the output document. [stdout]"
		};
		static readonly CommandOption outputDir = new CommandOption
		{
			Name = "output-dir",
			Help = "Output directory for generated HTML and images. Images will be stored in separate files. \nMutually exclusive with output-path."
		};
		static readonly CommandOptionList styleMapPath = new CommandOptionList
		{
			Name = "style-map",
			Help = "File containg a style map."
		};

		static readonly Command command = new Command(application: "Mammoth.Cli")
		{
			Parameters = new ICommandParameter[] { docPath, outputPath },
			Options = new ICommandOption[] { outputDir, styleMapPath }
		};

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
			if (!command.TryParse(args))
			{
				Environment.ExitCode = 1;
#if DEBUG
				Console.ReadKey();
#endif
				return;
			}

			var converter = new DocumentConverter();
			if (styleMapPath.HasValue)
			{
				string styleMap = File.ReadAllText(styleMapPath);
				converter = converter.AddStyleMap(styleMap);
			}
			if (outputDir.HasValue)
			{
				string htmlName = Path.GetFileName(outputPath.Value) ?? (Path.GetFileNameWithoutExtension(docPath.Value) + ".html");
				outputPath.Value = Path.Combine(outputDir, htmlName);
				int imageIndex = 0;
				converter = converter.ImageConverter(element =>
				{
					imageIndex++;
					string extension = element.ContentType.Split(new[] { '/' }, 2)[1];
					string filename = $"{imageIndex}.{extension}";

					using (var stream = element.Open())
					using (var file = File.Create(Path.Combine(outputDir, filename)))
					{
						stream.CopyTo(file);
					}
					return new Dictionary<string, string>
					{
						["src"] = filename
					};
				});
			}

			var result = converter.ConvertToHtml(docPath);
			foreach (string warning in result.Warnings)
				command.Warning(warning);

			if (outputPath.Value != null)
				File.WriteAllText(outputPath, result.Value);
			else
				Console.WriteLine(result.Value);
		}

		static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			switch (e.ExceptionObject)
			{
				case Exception ex:
					command.Error(ex.Message);
#if DEBUG
					command.Info(ex.ToString());
#endif
					break;
				default:
					command.Error(e.ExceptionObject.ToString());
					break;
			}
#if DEBUG
			Console.ReadKey();
#endif
			Environment.Exit(2);
		}
	}
}
