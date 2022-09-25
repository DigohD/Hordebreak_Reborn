using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Refinement
{
	[XmlType("RefinementRecipeData")]
	public class RefinementRecipeData : DataDef
	{
		[XmlElement("processNameRef")]
		public string processNameRef;

		[XmlElement("processDescriptionRef")]
		public string processDescriptionRef;

		[XmlElement("processIconRef")]
		public string processIconRef;

		[XmlElement("processTime")]
		public int processTime;

		[XmlArray("requiredMaterials")]
		[XmlArrayItem(typeof(MaterialDef))]
		public List<MaterialDef> requiredMaterials { get; set; }

		[XmlArray("producedMaterials")]
		[XmlArrayItem(typeof(MaterialDef))]
		public List<MaterialDef> producedMaterials { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(processNameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processNameRef '{processNameRef}' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(processNameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processNameRef '{processNameRef}' for '{Id}' not found in databank."));

			if (string.IsNullOrEmpty(processDescriptionRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processDescriptionRef '{processDescriptionRef}' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(processDescriptionRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processDescriptionRef '{processDescriptionRef}' for '{Id}' not found in databank."));

			if (string.IsNullOrEmpty(processIconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processIconRef '{processIconRef}' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<SpriteData>(processIconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"processIconRef '{processIconRef}' for '{Id}' not found in databank."));

			if (requiredMaterials == null || requiredMaterials.Count == 0)
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"requiredMaterials '{requiredMaterials}' for '{Id}' was null or had 0 entries."));
			else
			{
				foreach (var materialDef in requiredMaterials)
				{
                    materialDef.ValidateXMLData(
                        errorMessages,
                        Id,
                        fileName
                    );
                }
			}

			if (producedMaterials == null || producedMaterials.Count == 0)
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'producedMaterials' for '{Id}' was null or had 0 entries."));
			else
			{
				foreach (var materialDef in producedMaterials)
				{
                    materialDef.ValidateXMLData(
                        errorMessages,
                        Id,
                        fileName
                    );
                }
			}

			return errorMessages.Count > 0;
		}
	}
}