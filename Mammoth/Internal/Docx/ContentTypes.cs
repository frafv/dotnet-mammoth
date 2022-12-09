using System;
using System.Collections.Generic;
using System.IO;

namespace Mammoth.Internal.Docx
{
	internal class ContentTypes
	{
		internal static readonly ContentTypes DEFAULT = new ContentTypes();
		readonly static IDictionary<string, string> imageExtensions = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "png", "png" },
			{ "gif", "gif" },
			{ "jpeg", "jpeg" },
			{ "jpg", "jpeg" },
			{ "bmp", "bmp" },
			{ "tif", "tiff" },
			{ "tiff", "tiff" }
		};
		readonly IDictionary<string, string> extensionDefaults;
		readonly IDictionary<string, string> overrides;
		private ContentTypes()
			: this(
				new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase),
				new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase))
		{ }
		internal ContentTypes(IDictionary<string, string> extensionDefaults, IDictionary<string, string> overrides)
		{
			this.extensionDefaults = new Dictionary<string, string>(extensionDefaults, StringComparer.InvariantCultureIgnoreCase);
			this.overrides = new Dictionary<string, string>(overrides, StringComparer.InvariantCultureIgnoreCase);
		}
		public string FindContentType(string path)
		{
			if (overrides.TryGetValue(path, out string imageType))
				return imageType;
			else
			{
				string extension = Path.GetExtension(path)?.TrimStart('.');
				if (extensionDefaults.TryGetValue(extension, out imageType))
					return imageType;
				else
					return imageExtensions.TryGetValue(extension, out imageType) ? "image/" + imageType : null;
			}
		}
	}
}

