using System;
using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Docx
{
	internal class Relationships
	{
		internal static readonly Relationships EMPTY = new Relationships(Enumerable.Empty<Relationship>());
		internal IDictionary<string, string> targetsByRelationshipId;
		internal IDictionary<string, string[]> targetsByType;
		internal Relationships(IEnumerable<Relationship> relationships)
		{
			this.targetsByRelationshipId = relationships.ToDictionary(
				relationship => relationship.RelationshipId,
				relationship => relationship.Target);
			this.targetsByType = relationships.GroupBy(relationship => relationship.Type).ToDictionary(
				g => g.Key,
				g => g.Select(relationship => relationship.Target).ToArray());
		}
		public string FindTargetByRelationshipId(string relationshipId)
		{
			return targetsByRelationshipId.TryGetValue(relationshipId, out string target) ? target :
				throw new Exception($"Could not find relationship '{relationshipId}'");
		}
		public string[] FindTargetsByType(string type)
		{
			return targetsByType.TryGetValue(type, out string[] targets) ? targets : new string[0];
		}
	}
}

