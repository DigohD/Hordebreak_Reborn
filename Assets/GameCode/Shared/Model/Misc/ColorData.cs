using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Misc
{
	[XmlType("ColorData")]
	public class ColorData : DataDef
	{
		[XmlElement("colorCode")]
		public string colorCode { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (!ColorUtility.TryParseHtmlString(colorCode, out Color color))
				errorMessages.Add(new Tuple<string, string>("Error: colorCode", $"colorCode {colorCode} didn't return a color."));

			return errorMessages.Count > 0;
		}
	}
}