using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using FNZ.Shared.Model.World.Tile;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Building
{
	[XmlType("BuildingData")]
	public class BuildingData : DataDef
	{
		[XmlArray("requiredMaterials")]
		[XmlArrayItem("MaterialDef")]
		public List<MaterialDef> requiredMaterials { get; set; }

		[XmlElement("productRef")]
		public string productRef { get; set; }

		[XmlElement("categoryRef")]
		public string categoryRef { get; set; }

		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("descriptionRef")]
		public string descriptionRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		[XmlArray("unlockRefs")]
		[XmlArrayItem("unlockRef")]
		public List<string> unlockRefs { get; set; }

		[XmlArray("validTiles")]
		[XmlArrayItem("tileRef")]
		public List<string> validTiles { get; set; }

		[XmlElement("isWallAddon")]
		public bool isWallAddon { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (requiredMaterials == null || requiredMaterials.Count == 0)
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"'requiredMaterials' for BuildingData '{Id}' must exist and not be empty."));
			else
			{
				foreach (var md in requiredMaterials)
				{
					md.ValidateXMLData(
						errorMessages,
						Id,
						fileName
					);
				}
			}

			if (string.IsNullOrEmpty(productRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'productRef' of BuildingData '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.IsIdOfType<TileData>(productRef))
			{
				//not an entity
				if (!DataBank.Instance.IsIdOfType<FNEEntityData>(productRef))
				{
					// not a wall mounted object
					if (!DataBank.Instance.IsIdOfType<MountedObjectData>(productRef))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"BuildingData '{Id}' has invalid productRef: {productRef} - this is not an id for a TileData, MountedObjectData or FNEEntityData."));
				}
				else
				{
					var entityData = DataBank.Instance.GetData<FNEEntityData>(productRef);
					if (isWallAddon && !entityData.entityType.Equals("EdgeObject"))
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"BuildingData '{Id}' isWallAddon is set to true but it's product is not an edge object FNEEntityData. This is not allowed."));
				}
			}

			XMLValidation.ValidateId<BuildingCategoryData>(
				errorMessages,
				categoryRef,
				false,
				fileName,
				Id,
				"categoryRef"
			);

			XMLValidation.ValidateId<StringData>(
				errorMessages,
				nameRef,
				false,
				fileName,
				Id,
				"nameRef"
			);

			XMLValidation.ValidateId<StringData>(
			   errorMessages,
			   descriptionRef,
			   false,
			   fileName,
			   Id,
			   "descriptionRef"
		   );

			XMLValidation.ValidateId<SpriteData>(
			   errorMessages,
			   iconRef,
			   false,
			   fileName,
			   Id,
			   "iconRef"
		   );

			XMLValidation.ValidateIdList<BuildingData>(
				errorMessages,
				unlockRefs,
				false,
				fileName,
				Id,
				"unlockRefs"
			);

			return errorMessages.Count > 0;
		}
	}
}