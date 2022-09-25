using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.ItemTransferComponent
{
	[XmlType("ItemTransferComponentData")]
	public class ItemTransferComponentData : DataComponent 
	{
		[XmlElement("transferIntervalTicks")]
		public byte transferIntervalTicks;

		public override Type GetComponentType()
		{
			return typeof(ItemTransferComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return true;
		}
	}
}
