using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Items.Components
{

	public enum ModColor
	{
		[XmlEnum("White")]
		WHITE = 0,
		[XmlEnum("Black")]
		BLACK = 1,
		[XmlEnum("Green")]
		GREEN = 2,
		[XmlEnum("Blue")]
		BLUE = 3,
		[XmlEnum("Yellow")]
		YELLOW = 4,
		[XmlEnum("Red")]
		RED = 5,
		[XmlEnum("Brown")]
		BROWN = 6
	}

	public enum ModBuff
	{
		[XmlEnum("Damage")]
		MOD_DAMAGE = 0,
		[XmlEnum("FireRate")]
		MOD_FIRE_RATE = 1,
		[XmlEnum("ClipSize")]
		MOD_CLIP_SIZE = 2,
		[XmlEnum("ReloadTime")]
		MOD_RELOAD_TIME = 3,
		[XmlEnum("ProjectileSpeed")]
		MOD_PROJECTILE_SPEED = 4,

		[XmlEnum("DeathEffect")]
		MOD_DEATH_EFFECT = 5,
		[XmlEnum("AdditionalPellets")]
		MOD_ADDITIONAL_PELLETS = 6,
		[XmlEnum("Spread")]
		MOD_SPREAD = 7,
	}

	[XmlType("Moddata")]
	public class ModData{

		[XmlElement("buffType")]
		public ModBuff modBuff;

		[XmlElement("mulMod")]
		public float mulMod;

		[XmlElement("flatMod")]
		public float flatMod;

		[XmlElement("effectRef")]
		public string effectRef;
	}

	[XmlType("WeaponModItemComponentData")]
    public class ItemWeaponModComponentData : ItemComponentData
    {

		[XmlElement("modColor")]
		public ModColor modColor;

		[XmlArray("modBuffs")]
		[XmlArrayItem("ModBuff", typeof(ModData))]
		public List<ModData> modBuffList;

		public override Type GetComponentType()
        {
			return typeof(ItemWeaponModComponent);
        }

        public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
        {
			return true;
        }
    }

	public class ItemWeaponModComponent : ItemComponent
	{
		public ItemWeaponModComponentData Data
		{
			get
			{
                return (ItemWeaponModComponentData) base.m_Data;
			}
		}

		public override void Init()
		{ }

		public static Color GetModColorColor(ModColor modColor)
        {
			switch (modColor)
			{
				case ModColor.WHITE:
					return Color.white;

				case ModColor.BLACK:
					return Color.grey;

				case ModColor.BLUE:
					return Color.blue;

				case ModColor.BROWN:
					return new Color(0.28f, 0.2f, 0.14f);

				case ModColor.GREEN:
					return Color.green;

				case ModColor.RED:
					return Color.red;

				case ModColor.YELLOW:
					return Color.yellow;

				default:
					return Color.magenta;
			}
		}
	}
}