using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items.Components
{
	[XmlType("FuelItemComponentData")]
	public class ItemFuelComponentData : ItemComponentData
	{
		[XmlElement("fuelValue")]
		public int fuelValue { get; set; }

		public override Type GetComponentType()
		{
			return typeof(ItemFuelComponent);
		}

		public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
		{
			return false;
		}
	}

	public class ItemFuelComponent : ItemComponent
	{
		public ItemFuelComponentData Data
		{
			get
			{
				return (ItemFuelComponentData)base.m_Data;
			}
		}

		public override void Init()
		{ }
	}
}