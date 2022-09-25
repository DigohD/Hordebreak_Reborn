using FNZ.Shared.Model.SFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Door
{
	[XmlType("DoorComponentData")]
	public class DoorComponentData : DataComponent
	{
		[XmlElement("openAnimationName")]
		public string openAnimationName { get; set; }

		[XmlElement("closeAnimationName")]
		public string closeAnimationName { get; set; }

		[XmlElement("openSFXRef")]
		public string openSFXRef { get; set; }

		[XmlElement("closeSFXRef")]
		public string closeSFXRef { get; set; }

		public override Type GetComponentType()
		{
			return typeof(DoorComponentShared);
		}

        public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
        {
			var compName = this.GetType().ToString().Split('.').Last();

			XMLValidation.ValidateId<SFXData>(
			   errorMessages,
			   openSFXRef,
			   true,
			   fileName,
			   parentId,
			   "openSFXRef",
			   compName
		   );

			XMLValidation.ValidateId<SFXData>(
			   errorMessages,
			   closeSFXRef,
			   true,
			   fileName,
			   parentId,
			   "closeSFXRef",
			   compName
		   );

			return true;
        }
    }
}