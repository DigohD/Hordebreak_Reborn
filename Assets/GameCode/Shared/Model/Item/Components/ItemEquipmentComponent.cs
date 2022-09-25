using FNZ.Shared.Model.Entity.Components.Health;
using FNZ.Shared.Model.Entity.EntityViewData;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Items.Components
{
	public delegate void OnTrigger(Item triggeringItem, string effectRef);
	public delegate void OnReload(Item reloadingItem, string effectRef);

	public enum EquipmentType
	{
		None = 0,

		Weapon = 1,

		[XmlEnum(Name = "Head")]
		Head = 2,
		[XmlEnum(Name = "Torso")]
		Torso = 3,
		[XmlEnum(Name = "Legs")]
		Legs = 4,
		[XmlEnum(Name = "Waist")]
		Waist = 5,
		[XmlEnum(Name = "Hands")]
		Hands = 6,
		[XmlEnum(Name = "Back")]
		Back = 7,
		[XmlEnum(Name = "Feet")]
		Feet = 8,

		Trinket = 9,

		Consumable = 10,
	}

	public abstract class ItemEquipmentComponentData : ItemComponentData
	{
		[XmlElement("triggersPerMinute")]
		public int TriggersPerMinute { get; set; } = 120;

		[XmlElement("itemMeshRef")]
		public string ItemMeshRef { get; set; }
		
		[XmlElement("itemTextureRef")]
		public string ItemTextureRef { get; set; }

		[XmlArray("statModList")]
		[XmlArrayItem(typeof(StatModData))]
		public List<StatModData> statMods;

		public abstract EquipmentType GetEquipmentType();

		public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			var compName = this.GetType().ToString().Split('.').Last();

			XMLValidation.ValidateId<FNEEntityMeshData>(
				errorMessages,
				ItemMeshRef,
				false,
				fileName,
				parentId,
				"itemMeshData",
				compName
			);

			return true;
		}
	}

	public abstract class ItemEquipmentComponent : ItemComponent
	{
		public ItemEquipmentComponentData Data
		{
			get
			{
				return (ItemEquipmentComponentData)base.m_Data;
			}
		}

		public OnTrigger d_OnTrigger;
		public OnTrigger d_OnReload;

		public float FinalFireRate;

		public float GetStartCooldownTime()
		{
			if (FinalFireRate != 0)
				return 60f / FinalFireRate;
			
			return 60f / Data.TriggersPerMinute;
		}

		public float CooldownTimer = 0f;

		public abstract void Tick(float deltaTime);
		public abstract void OnActivate();
		public abstract void OnDeactivate();
	}
}