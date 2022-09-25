using System;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	[XmlType("StatModData")]
	public partial class StatModData : DataComponent
	{
		public const string MAX_HEALTH = "MaxHealth";
		public const string ARMOR = "Armor";
		public const string MAX_SHIELDS = "MaxShields";

		[XmlElement("modType")]
		public StatType modType;

		[XmlElement("amount")]
		public float amount;

		public override Type GetComponentType()
		{
			return typeof(StatComponentShared);
		}
	}
}
