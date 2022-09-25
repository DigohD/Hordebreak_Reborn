using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.SFX;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Model.String;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items
{
	[XmlType("ItemData")]
	public class ItemData : DataDef
	{
		//public List<ItemComponent> m_Components = new List<ItemComponent>();		Seems this one has no references. Delete it? /Johan

		[XmlElement("nameRef")]
		public string nameRef { get; set; }

		[XmlElement("iconRef")]
		public string iconRef { get; set; }

		[XmlElement("infoRef")]
		public string infoRef { get; set; }

		[XmlElement("height")]
		public int height { get; set; }

		[XmlElement("width")]
		public int width { get; set; }

		[XmlElement("maxStackSize")]
		public int maxStackSize { get; set; }

		[XmlElement("pickupSoundRef")]
		public string pickupSoundRef { get; set; }

		[XmlElement("laydownSoundRef")]
		public string laydownSoundRef { get; set; }

		[XmlArray("components")]
		[XmlArrayItem(typeof(ItemComponentData))]
		public List<ItemComponentData> components { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(nameRef))
				errorMessages.Add(new Tuple<string, string>("Error: displayNameRef", $"displayNameRef for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<StringData>(nameRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"displayNameRef '{nameRef}' for {Id} ws not found in database."));

			bool gate = false;
			foreach (var comp in components)
			{
				comp.ValidateXMLData(errorMessages, Id, fileName);

				if (comp is ItemClothingComponentData || comp is ItemWeaponComponentData)
				{
					gate = true;
					break;
				}
			}

			if (!gate)
			{
				if (string.IsNullOrEmpty(iconRef))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"itemIconSpriteName for {Id} must exist and must not be empty."));
				else if (!DataBank.Instance.DoesIdExist<SpriteData>(iconRef))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"itemIconSpriteName '{iconRef}' for {Id} was not found in database."));
			}

			if (infoRef != null && infoRef != string.Empty)
			{
				if (!DataBank.Instance.DoesIdExist<StringData>(infoRef))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"infoRef '{infoRef}' for {Id} was not found in database."));
			}

			if (pickupSoundRef != null && pickupSoundRef != string.Empty)
			{
				if (!DataBank.Instance.DoesIdExist<SFXData>(pickupSoundRef))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"pickupSoundRef '{pickupSoundRef}' for {Id} was not found in database."));
			}

			if (laydownSoundRef != null && laydownSoundRef != string.Empty)
			{
				if (!DataBank.Instance.DoesIdExist<SFXData>(laydownSoundRef))
					errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"laydownSoundRef '{pickupSoundRef}' for {Id} was not found in database."));
			}

			return errorMessages.Count > 0;
		}

		public bool HasComponent<T>()
		{
			return components.Find(c => c is T) != null;
		}

		public ItemComponentData GetComponentData<T>()
		{
			foreach (var comp in components)
			{
				if (comp is T)
					return comp;
			}

			return null;
		}
	}
}