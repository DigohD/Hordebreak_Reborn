using FNZ.Shared.Model.VFX;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.EntityViewData
{
	public class EntityVFXData
	{
		[XmlElement("vfxRef")]
		public string vfxRef { get; set; }

		[XmlElement("alwaysOn")]
		public bool alwaysOn { get; set; } = false;

		[XmlElement("scaleMod")]
		public float scaleMod { get; set; }

		[XmlElement("offsetX")]
		public float offsetX { get; set; }

		[XmlElement("offsetY")]
		public float offsetY { get; set; }

		[XmlElement("offsetZ")]
		public float offsetZ { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName = "def")
        {
            XMLValidation.ValidateId<VFXData>(
                errorMessages,
                vfxRef,
                false,
                fileName,
                parentId,
                "vfxRef",
                "EntityVFXData"
            );
        }
    }
}