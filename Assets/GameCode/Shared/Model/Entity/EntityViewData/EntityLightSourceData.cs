using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.EntityViewData
{
	public class EntityLightSourceData
	{
		public enum LightType
        {
			Point = 0,
			Spot = 1,
        };

		[XmlElement("lightType")]
		public LightType lightType { get; set; }

		[XmlElement("intensity")]
		public float intensity { get; set; }

		[XmlElement("range")]
		public float range { get; set; }

		[XmlElement("color")]
		public string color { get; set; }

		[XmlElement("spotOuterAngle")]
		public float spotOuterAngle { get; set; }

		[XmlElement("spotInnerAnglePercent")]
		public float spotInnerAnglePercent { get; set; }

		[XmlElement("offsetX")]
		public float offsetX { get; set; }

		[XmlElement("offsetY")]
		public float offsetY { get; set; }

		[XmlElement("offsetZ")]
		public float offsetZ { get; set; }

		[XmlElement("rotationX")]
		public float rotationX { get; set; }

		[XmlElement("rotationY")]
		public float rotationY { get; set; }

		[XmlElement("rotationZ")]
		public float rotationZ { get; set; }

		[XmlElement("flickerIntensity")]
		public float flickerIntensity { get; set; }
		
		[XmlElement("flickerSmoothness")]
		public float flickerSmoothness { get; set; }
		
		[XmlElement("flickerIntervalMin")]
		public float flickerIntervalMin { get; set; }
		
		[XmlElement("flickerIntervalMax")]
		public float flickerIntervalMax { get; set; }

        public void ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName, string compName = "def")
        {
            XMLValidation.ValidateColor(
                errorMessages,
                color,
                false,
                fileName,
                parentId,
                "color",
                "EntityLightSourceData"
            );
        }
    }
}