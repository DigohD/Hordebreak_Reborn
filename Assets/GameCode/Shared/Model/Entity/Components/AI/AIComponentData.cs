using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.AI
{
	[XmlType("AIComponentData")]
	public class AIComponentData : DataComponent
	{
		public override Type GetComponentType()
		{
			return typeof(AIComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return false;
		}
	}
}
