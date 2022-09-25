using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity
{
	[XmlType("FNEEntityData")]
	public class FNEEntityData : DataDef
	{
		[XmlAttribute("entityType")]
		public string entityType { get; set; }

		[XmlArray("components")]
		[XmlArrayItem(typeof(DataComponent))]
		public List<DataComponent> components { get; set; }

		[XmlElement("pathingCost")]
		public byte pathingCost { get; set; }

		[XmlElement("seeThrough")]
		public bool seeThrough { get; set; }

		[XmlElement("seeThroughRange")] 
		public byte seeThroughRange { get; set; } = 0;

		[XmlElement("hittable")]
		public bool hittable { get; set; }

		[XmlElement("blocking")]
		public bool blocking { get; set; }

		[XmlElement("blocksBuilding")]
		public bool blocksBuilding { get; set; } = true;

		[XmlElement("smallCollisionBox")]
		public bool smallCollisionBox { get; set; } = false;

		[XmlElement("editorName")]
		public string editorName { get; set; }

		[XmlElement("editorCategoryName")]
		public string editorCategoryName { get; set; } = "Misc";

		[XmlArray("entityViewVariations")]
		[XmlArrayItem("viewRef", typeof(string))]
		public List<string> entityViewVariations { get; set; }

		[XmlElement("viewRef")] 
		public string ViewRef { get; set; }

		// Only used by edge objects and tiles
		[XmlArray("roomPropertyRefs")]
		[XmlArrayItem("propertyRef", typeof(string))]
		public List<string> roomPropertyRefs { get; set; }

		// Only used by edge objects
		[XmlElement("isMountable")]
		public bool isMountable { get; set; }
		
		// Only used by tile objects
		[XmlElement("blocksTileBuilding")] 
		public bool blocksTileBuilding { get; set; } = true;

		public T GetComponentData<T>() where T : DataComponent
		{
			foreach (var comp in components)
			{
				if (comp is T) return comp as T;
			}

			return default;
		}

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (entityType != EntityType.PLAYER && entityType != EntityType.ECS_ENEMY && entityType != EntityType.GO_ENEMY)
			{
                if (entityViewVariations != null && entityViewVariations.Count > 0)
                {
                    XMLValidation.ValidateIdList<FNEEntityViewData>(
                       errorMessages,
                       entityViewVariations,
                       false,
                       fileName,
                       Id,
                       "entityViewVariations"
                   );
                }
                else
                {
                    errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"entityViewVariations of {Id} must be specified."));
                }
            }

			if (entityType == EntityType.EDGE_OBJECT)
			{
				if (roomPropertyRefs.Count > 0)
				{
					foreach (var property in roomPropertyRefs)
					{
						if (!DataBank.Instance.DoesIdExist<RoomPropertyData>(property))
							errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"property '{property}' of {Id} was not found in database."));
					}
				}
			}

            foreach(var compData in components)
            {
                compData.ValidateComponentXMLData(errorMessages, Id, fileName);
            }

            if (roomPropertyRefs != null && roomPropertyRefs.Count > 0)
            {
                XMLValidation.ValidateIdList<RoomPropertyData>(
                    errorMessages,
                    roomPropertyRefs,
                    false,
                    fileName,
                    Id,
                    "roomPropertyRefs"
                );
            }
                
            return errorMessages.Count > 0;
		}
	}
}

