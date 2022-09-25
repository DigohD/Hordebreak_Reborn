using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items.Components
{

	[XmlType("ClothingItemComponentData")]
	public class ItemClothingComponentData : ItemEquipmentComponentData
	{
		[XmlElement("equipmentType")]
		public EquipmentType Type;

		public override Type GetComponentType()
		{
			return typeof(ItemClothingComponent);
		}

		public override EquipmentType GetEquipmentType()
		{
			return Type;
		}

		public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
		{
			base.ValidateXMLData(errorMessages, Id, fileName);

			return errorMessages.Count > 0;
		}
	}

	public class ItemClothingComponent : ItemEquipmentComponent
	{
		new public ItemClothingComponentData Data
		{
			get
			{
				return (ItemClothingComponentData)base.m_Data;
			}
		}

		public override void Init()
		{ }

		public override void Serialize(NetBuffer bw)
		{ }

		public override void Deserialize(NetBuffer br)
		{ }

		public override void Tick(float deltaTime)
		{ }

		public override void OnActivate()
		{ }

		public override void OnDeactivate()
		{ }
	}
}