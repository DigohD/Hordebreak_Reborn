using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Building
{
	[XmlType("BuildingCategoryData")]
	public class BuildingCategoryData : DataDef
	{
		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		[XmlElement("preferredIndex")]
		public int preferredIndex { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

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

            return errorMessages.Count > 0;
		}
	}

}