using System;

namespace Mammoth.Internal.Styles.Parsing
{
	internal class ParseException : Exception
	{
		internal ParseException(string message) : base(message)
		{
		}
	}
}
