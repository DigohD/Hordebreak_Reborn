using FNZ.Shared.Model.Effect;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Items.Components
{
	public enum WeaponPosture
	{
		[XmlEnum("Rifle")]
		RIFLE = 0,
		[XmlEnum("Heavy")]
		HEAVY = 1,
		[XmlEnum("Light")]
		LIGHT = 2,
		[XmlEnum("Throw")]
		THROW = 3,

		[XmlEnum("Unarmed")]
		UNARMED = 4,
	}

	[XmlType("ModSlotGenData")]
	public class ModSlotGenData
	{
		[XmlElement("chanceInPercent")]
		public float chanceInPercent { get; set; }

		[XmlArray("modTypeGen")]
		[XmlArrayItem("ModTypeGenData", typeof(ModTypeGenData))]
		public List<ModTypeGenData> ModTypeGenList { get; set; }
	}

	[XmlType("ModTypeGenData")]
	public class ModTypeGenData
	{
		[XmlElement("weight")]
		public int weight { get; set; }

		[XmlElement("modColor")]
		public ModColor modColor { get; set; }
	}

	[XmlType("WeaponItemComponentData")]
	public class ItemWeaponComponentData : ItemEquipmentComponentData
	{

		[XmlElement("isAutomatic")]
		public bool isAutomatic { get; set; }

		[XmlElement("reloadTimeInSeconds")]
		public float reloadTimeInSeconds { get; set; }

		[XmlElement("soundRange")]
		public int soundRange { get; set; }

		[XmlElement("ammoClipSize")]
		public int ammoClipSize { get; set; }

		[XmlElement("reloadEffectRef")]
		public string reloadEffectRef { get; set; }

		[XmlElement("effectRef")]
		public string effectRef { get; set; }

		[XmlElement("weaponPosture")]
		public WeaponPosture weaponPosture;

		[XmlElement("muzzleOffsetForward")]
		public float muzzleOffsetX { get; set; }

		[XmlElement("muzzleOffsetRight")]
		public float muzzleOffsetY { get; set; }

		[XmlElement("muzzleOffsetUp")]
		public float muzzleOffsetZ { get; set; }

		[XmlElement("scaleMod")]
		public float scaleMod { get; set; } = 1;

		[XmlElement("iconOffsetRight")]
		public float iconOffsetRight { get; set; }

		[XmlElement("iconOffsetUp")]
		public float iconOffsetUp { get; set; }

		[XmlElement("iconScaleMod")]
		public float iconScaleMod { get; set; } = 1;

		[XmlArray("modSlotGen")]
		[XmlArrayItem("ModSlotGenData", typeof(ModSlotGenData))]
		public List<ModSlotGenData> ModSlotGenList { get; set; }

		public override Type GetComponentType()
		{
			return typeof(ItemWeaponComponent);
		}

		public override EquipmentType GetEquipmentType()
		{
			return EquipmentType.Weapon;
		}

		public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
		{
			base.ValidateXMLData(errorMessages, Id, fileName);

			if (string.IsNullOrEmpty(effectRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"Entry 'effectRef' for {Id} must exist and must not be empty."));
			else if (!DataBank.Instance.DoesIdExist<EffectData>(effectRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"effectRef '{effectRef}' for {Id} was not found in database."));

			if (reloadEffectRef != null && !DataBank.Instance.DoesIdExist<EffectData>(reloadEffectRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"reloadEffectRef '{reloadEffectRef}' for {Id} was not found in database."));

			return errorMessages.Count > 0;
		}	}

	public struct WeaponMod
    {
		public ModColor ModColor;
		public Item ModItem;

		public void Serialize(NetBuffer bw)
        {
			bw.Write((byte) ModColor);
			bw.Write(ModItem != null);
			if(ModItem != null)
            {
				var IdCode = IdTranslator.Instance.GetIdCode<ItemData>(ModItem.Data.Id);
				bw.Write(IdCode);
            }
        }

		public void Deserialize(NetBuffer br)
		{
			ModColor = (ModColor) br.ReadByte();
			if (br.ReadBoolean())
			{
				var id = IdTranslator.Instance.GetId<ItemData>(br.ReadUInt16());
				ModItem = Item.GenerateItem(id);
			}
		}
	}

	public class ItemWeaponComponent : ItemEquipmentComponent
	{
		new public ItemWeaponComponentData Data
		{
			get
			{
				return (ItemWeaponComponentData) base.m_Data;
			}
		}

		public int CurrentAmmoInClip;

		public float ReloadTimer = 0f;

		private bool m_IsActive;
		public bool Reloading;

		public List<WeaponMod> ModSlots = new List<WeaponMod>();

		public float FinalReloadTime;
		public int FinalClipSize;
		
		public override void Init()
		{ 

		}

		public void GenerateModSlots()
        {
			foreach(var modGen in Data.ModSlotGenList)
            {
				var rng = FNERandom.GetRandomFloatInRange(0, 100);
				if(rng < modGen.chanceInPercent)
                {
					int totalWeight = 0;

					foreach (var typeGen in modGen.ModTypeGenList)
					{
						totalWeight += typeGen.weight;
					}

					var modRandom = FNERandom.GetRandomIntInRange(1, totalWeight + 1);

					foreach (var typeGen in modGen.ModTypeGenList)
					{
						if (modRandom <= typeGen.weight)
						{
							ModSlots.Add(
								new WeaponMod
                                {
									ModColor = typeGen.modColor,
									ModItem = null
								}	
							);
							break;
						}
						else
						{
							modRandom -= typeGen.weight;
						}
					}
				}
            }
        }

		public string[] GetModItemIdsArray()
		{
			var modItemIds = new string[ModSlots.Count];
			for (int i = 0; i < modItemIds.Length; i++)
			{
				modItemIds[i] = ModSlots[i].ModItem?.Data.Id;
			}
			return modItemIds;
		}

		public ItemWeaponModComponentData[] GetModComponentArray()
		{
			var mods = new ItemWeaponModComponentData[ModSlots.Count];
			for(int i = 0; i < mods.Length; i++)
			{
				mods[i] = ModSlots[i].ModItem?.GetComponent<ItemWeaponModComponent>().Data;
			}
			return mods;
		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(ModSlots.Count);
            foreach (var modSlot in ModSlots)
            {
				modSlot.Serialize(bw);
            }
		}

		public override void Deserialize(NetBuffer br)
		{
			ModSlots.Clear();

			var count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
				var modData = new WeaponMod();
				modData.Deserialize(br);
				ModSlots.Add(modData);
            }
		}

		public override void OnActivate()
		{
			if (!Data.isAutomatic && CooldownTimer <= 0 && !Reloading)
			{
				Trigger();
				m_IsActive = false;
			}
			else if (CooldownTimer <= 0 && !Reloading)
			{
				m_IsActive = true;
			}
		}

		public override void OnDeactivate()
		{
			m_IsActive = false;
		}

		public void Reload()
		{
			if (CurrentAmmoInClip == Data.ammoClipSize || Reloading)
				return;

			d_OnReload?.Invoke(ParentItem, Data.reloadEffectRef);
			ReloadTimer = FinalReloadTime;
			Reloading = true;
		}

		public override void Tick(float deltaTime)
		{
			if (Reloading)
			{
				ReloadTimer -= deltaTime;
				if (ReloadTimer <= 0)
				{
					CurrentAmmoInClip = FinalClipSize;
					CooldownTimer = 0;
					Reloading = false;
				}
				return;
			}

			CooldownTimer -= deltaTime;

			if (CooldownTimer < 0)
			{
				CooldownTimer = 0;
				if (CurrentAmmoInClip <= 0)
				{
					Reload();
					return;
				}
			}

			if (m_IsActive && CooldownTimer <= 0)
			{
				Trigger();
			}
		}

		private void Trigger()
		{
			CurrentAmmoInClip--;
			d_OnTrigger?.Invoke(ParentItem, Data.effectRef);

			if (CurrentAmmoInClip <= 0)
			{
				Reload();
				return;
			}

			CooldownTimer = 60f / FinalFireRate;
		}

        public void RecalculateFinalModdedProperties()
        {
			int clipSizeFlatMod = 0;
			float clipSizeMulMod = 1;

			float fireRateMulMod = 1;

			float reloadTimeMulMod = 1;

			foreach (var mod in GetModComponentArray())
			{
				if (mod == null)
					continue;

				foreach (var buff in mod.modBuffList)
				{
					switch (buff.modBuff)
					{
						case ModBuff.MOD_CLIP_SIZE:
							clipSizeFlatMod += (int) buff.flatMod;
							clipSizeMulMod *= buff.mulMod;
							break;

						case ModBuff.MOD_FIRE_RATE:
							fireRateMulMod *= buff.mulMod;
							break;

						case ModBuff.MOD_RELOAD_TIME:
							reloadTimeMulMod *= buff.mulMod;
							break;
					}
				}
			}

			FinalClipSize = (int) ((float) Data.ammoClipSize * clipSizeMulMod) + clipSizeFlatMod;
			FinalFireRate = Data.TriggersPerMinute * fireRateMulMod;
			FinalReloadTime = Data.reloadTimeInSeconds * reloadTimeMulMod;
		}
	}
}