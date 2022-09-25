using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items.Components
{
	[XmlType("BurnableData")]
	public class BurnableData : DataDef
	{
		[XmlElement("nameRef")]
		public string nameRef;

		[XmlElement("iconRef")]
		public string iconRef;

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'displayNameRef' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayNameRef '{nameRef}' of {Id} was not found in database."));

			if (string.IsNullOrEmpty(iconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'uiIconRef' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<SpriteData>(iconRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"uiIconRef '{iconRef}' for {Id} was not found in database."));

			return errorMessages.Count > 0;
		}
	}

	[XmlType("BurnableItemComponentData")]
	public class ItemBurnableComponentData : ItemComponentData
	{
		[XmlElement("gradeRef")]
		public string gradeRef;

		[XmlElement("burnTime")]
		public int burnTime;

		public override Type GetComponentType()
		{
			return typeof(ItemBurnableComponent);
		}

		public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
		{
			if (string.IsNullOrEmpty(gradeRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'gradeRef' for '{Id}' must exist and not be empty."));
			else if (!DataBank.Instance.DoesIdExist<BurnableData>(gradeRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"gradeRef '{gradeRef}' was not found in database."));

			return errorMessages.Count > 0;
		}
	}

	public class ItemBurnableComponent : ItemComponent
	{
		public ItemBurnableComponentData Data
		{
			get
			{
				return (ItemBurnableComponentData)base.m_Data;
			}
		}

		public override void Init()
		{ }
	}
}