using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.RoomRequirements 
{

	[XmlType("RoomPropertyRequirementData")]
	public class RoomPropertyRequirementData
	{
		[XmlElement("propertyRef")]
		public string propertyRef { get; set; }

		[XmlElement("level")]
		public byte level { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName)
        {
            XMLValidation.ValidateId<RoomPropertyData>(
                errorMessages,
                propertyRef,
                false,
                fileName,
                parentId,
                "RoomPropertyRequirementData propertyRef",
                compName
            );
        }
    }
}