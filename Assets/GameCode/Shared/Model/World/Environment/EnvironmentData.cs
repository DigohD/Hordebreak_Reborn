using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Environment
{

	[XmlType("EnvironmentData")]
	public class EnvironmentData : DataDef
	{
		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayNameRef for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayNameRef '{nameRef}' for {Id} was not found in database."));

			if (string.IsNullOrEmpty(iconRef))
				errorMessages.Add(new Tuple<string, string>("Error: iconRef", $"iconRef for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<SpriteData>(iconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"iconRef '{iconRef}' for {Id} was not found in database."));

			return errorMessages.Count > 0;
		}
	}
}