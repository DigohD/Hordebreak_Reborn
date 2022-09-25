using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Consumer
{
	[XmlType("ResourceConsumptionData")]
	public class ResourceConsumptionData
	{
		[XmlElement("resourceRef")]
		public string resourceRef { get; set; }

		[XmlElement("amount")]
		public byte amount { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName)
        {
            XMLValidation.ValidateId<RoomResourceData>(
                errorMessages,
                resourceRef,
                false,
                fileName,
                parentId,
                "ResourceConsumptionData resourceRef",
                compName
            );
        }
    }
}