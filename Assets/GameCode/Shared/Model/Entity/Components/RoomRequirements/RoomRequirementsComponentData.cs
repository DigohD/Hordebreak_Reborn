using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.RoomRequirements 
{

	[XmlType("RoomRequirementsComponentData")]
	public class RoomRequirementsComponentData : DataComponent
	{
		// Only used by edge objects and tiles
		[XmlArray("propertyRequirements")]
		[XmlArrayItem(typeof(RoomPropertyRequirementData))]
		public List<RoomPropertyRequirementData> propertyRequirements { get; set; }

		[XmlElement("unsatisfiedMod")]
		public float unsatisfiedMod { get; set; }

		public override Type GetComponentType()
		{
			return typeof(RoomRequirementsComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            if (propertyRequirements != null && propertyRequirements.Count > 0)
            {
                foreach (var req in propertyRequirements)
                {
                    req.ValidateXMLData(
                        errorMessages,
                        parentId,
                        fileName,
                        compName
                    );
                }
            }
            else
            {
                errorMessages.Add(
                   new Tuple<string, string>(
                       $"Error in {fileName}",
                       $"propertyRequirements of '{compName}' in '{parentId}' must be specified."
                   )
                );
            }

            return false;
        }
    }
}