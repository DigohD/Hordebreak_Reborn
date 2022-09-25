using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.BuildingAddon
{
	[XmlType("BuildingAddonData")]
	public class BuildingAddonData : DataDef
	{
		[XmlArray("requiredMaterials")]
		[XmlArrayItem("MaterialDef")]
		public List<MaterialDef> requiredMaterials { get; set; }

		[XmlArray("refundedMaterials")]
		[XmlArrayItem("MaterialDef")]
		public List<MaterialDef> refundedMaterials { get; set; }

		[XmlElement("productRef")]
		public string productRef { get; set; }

		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		[XmlElement("addonColor")]
		public string addonColor { get; set; } = "#151515";

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (requiredMaterials != null)
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

			if (refundedMaterials != null)
			{
				foreach (var md in refundedMaterials)
				{
					md.ValidateXMLData(
						errorMessages,
						Id,
						fileName
					);
				}
			}

			XMLValidation.ValidateId<FNEEntityData>(
				errorMessages,
				productRef,
				false,
				fileName,
				Id,
				"productRef"
			);

			XMLValidation.ValidateId<StringData>(
				errorMessages,
				nameRef,
				false,
				fileName,
				Id,
				"nameRef"
			);

			XMLValidation.ValidateId<SpriteData>(
			   errorMessages,
			   iconRef,
			   false,
			   fileName,
			   Id,
			   "iconRef"
			);

			XMLValidation.ValidateColor(
				errorMessages,
				addonColor,
				true,
				fileName,
				Id,
				"addonColor"
			);

			return errorMessages.Count > 0;
		}
	}
}