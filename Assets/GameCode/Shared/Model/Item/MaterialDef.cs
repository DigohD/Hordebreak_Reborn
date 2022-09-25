using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items
{
	[XmlType("MaterialDef")]
	public class MaterialDef
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName = "def")
        {
            XMLValidation.ValidateId<ItemData>(
                errorMessages,
                itemRef,
                false,
                fileName,
                parentId,
                "MaterialDef itemRef",
                compName
            );
        }
    }

}