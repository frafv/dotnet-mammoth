using System.Linq;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal static class RelationshipsXml
	{
		public static Relationships ReadRelationshipsXmlElement(XmlElement element)
		{
			return new Relationships(
				element.FindChildren("relationships:Relationship")
					.Select(elem => ReadRelationship(elem)));
		}
		static Relationship ReadRelationship(XmlElement element)
		{
			string relationshipId = element.GetAttribute("Id");
			string target = element.GetAttribute("Target");
			string type = element.GetAttribute("Type");
			return new Relationship(relationshipId, target, type);
		}
	}
}

