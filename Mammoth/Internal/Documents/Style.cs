namespace Mammoth.Internal.Documents
{
	internal class Style
	{
		internal Style(string styleId, string name = null)
		{
			StyleId = styleId;
			Name = name;
		}
		public string StyleId { get; }

		public string Name { get; }

		public string Describe()
		{
			return Name != null ? $"{Name} (Style ID: {StyleId})" : "Style ID: " + StyleId;
		}
	}
}

