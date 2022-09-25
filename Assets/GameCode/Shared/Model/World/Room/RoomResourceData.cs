using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.World
{
	[XmlType("RoomResourceData")]
	public class RoomResourceData : DataDef
	{
		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }


		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayName for {Id} must exist and must not be empty."));

			return errorMessages.Count > 0;
		}
	}
}