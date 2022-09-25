using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.VFX 
{
	[XmlType("VFXLightData")]
	public class VFXLightData
	{
		[XmlElement("range")]
		public float range { get; set; } = 2;

		[XmlElement("intensity")]
		public float intensity { get; set; } = 500;

		[XmlElement("startColor")]
		public string startColor { get; set; } = "#FF0000";

		[XmlElement("endColor")]
		public string endColor { get; set; } = "#0000FF";

		[XmlElement("fadeTime")]
		public float fadeTime { get; set; } = 1;
	}
}