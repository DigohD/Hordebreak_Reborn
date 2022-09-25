using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.Entity.Components.PlayerViewSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Player
{
	[XmlType("PlayerComponentData")]
	public class PlayerComponentData : DataComponent
	{
		[XmlElement("baseSpeed")]
		public float baseSpeed;

		[XmlArray("startingBuildings")]
		[XmlArrayItem("buildingRef")]
		public List<string> startingBuildings { get; set; }

		[XmlArray("viewVariations")]
		[XmlArrayItem(typeof(PlayerViewData))]
		public List<PlayerViewData> viewVariations;

		public override Type GetComponentType()
		{
			return typeof(PlayerComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
            var compName = this.GetType().ToString().Split('.').Last();

            XMLValidation.ValidateIdList<BuildingData>(
                errorMessages,
                startingBuildings,
                false,
                fileName,
                parentId,
                "startingBuildings",
                compName
            );

            if(viewVariations == null || viewVariations.Count == 0)
                errorMessages.Add(new Tuple<string, string>(
                    $"Error in {fileName}",
                    $"property '{viewVariations}' of '{compName}' of '{parentId}' must be specified."));
            else
            {
                foreach (var vv in viewVariations)
                    vv.ValidateXMLData(
                        errorMessages,
                        parentId,
                        fileName,
                        compName
                    );
            }

            return true;
        }
    }
}
