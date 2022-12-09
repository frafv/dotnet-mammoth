using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal interface IHasChildren
	{
		IEnumerable<DocumentElement> Children { get; }
	}
}

