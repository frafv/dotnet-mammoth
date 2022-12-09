namespace Mammoth.Internal.Docx
{
	internal class Relationship
	{
		internal Relationship(string relationshipId, string target, string type)
		{
			this.RelationshipId = relationshipId;
			this.Target = target;
			this.Type = type;
		}
		public string RelationshipId { get; }

		public string Target { get; }

		public string Type { get; }
	}
}

