using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.InteractionEvent 
{
	[XmlType("InteractionEventComponentData")]
	public class InteractionEventComponentData : DataComponent
	{
		[XmlElement("interactionStringRef")]
		public string interactionStringRef { get; set; }

		[XmlElement("eventRef")]
		public string eventRef { get; set; }

		[XmlElement("effectRef")]
		public string effectRef { get; set; }
		
		[XmlElement("transformedEntityRef")]
		public string transformedEntityRef { get; set; }
		
		public override Type GetComponentType()
		{
			return typeof(InteractionEventComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return true;
		}
	}
}