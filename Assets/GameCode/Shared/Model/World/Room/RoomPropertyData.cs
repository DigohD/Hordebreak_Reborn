using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.World
{
	public class RoomPropertyData : DataDef
	{
		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		[XmlElement("displayNameRef")]
		public string displayNameRef { get; set; }

		[XmlElement("absencePropertyRef")]
		public string absencePropertyRef { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(iconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"iconRef for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<SpriteData>(iconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"iconRef '{iconRef}' for {Id} was not found in database."));

			if (string.IsNullOrEmpty(displayNameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayNameRef for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(displayNameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"iconRef '{iconRef}' for {Id} was not found in database."));

			if (absencePropertyRef != null && !DataBank.Instance.DoesIdExist<RoomPropertyData>(absencePropertyRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"absencePropertyRef '{absencePropertyRef}' for {Id} was not found in database."));

			return errorMessages.Count > 0;
		}
	}
}