using FNZ.Shared.Model.Effect;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.Items.Components
{
	public enum ConsumableBuff
	{
		NONE = 0,
		[XmlEnum("HealthGain")]
		HEALTH_GAIN,
		[XmlEnum("HealthLoss")]
		HEALTH_LOSS
	}

	[XmlType("ConsumableItemComponentData")]
	public class ItemConsumableComponentData : ItemEquipmentComponentData
	{
		[XmlElement("buff")]
		public ConsumableBuff buff { get; set; }

		[XmlElement("amount")]
		public float amount { get; set; }

		[XmlElement("effectRef")]
		public string effectRef { get; set; }

		[XmlElement("weaponPosture")]
		public WeaponPosture weaponPosture;
		
		[XmlElement("buildingRef")]
		public string buildingRef;

		public override Type GetComponentType()
		{
			return typeof(ItemConsumableComponent);
		}

        public override EquipmentType GetEquipmentType()
        {
			return EquipmentType.Consumable;
		}

        public override bool ValidateXMLData(List<Tuple<string, string>> errorMessages, string Id, string fileName)
		{
			// base.ValidateXMLData(errorMessages, Id, fileName);

			if (!string.IsNullOrEmpty(effectRef) && !DataBank.Instance.DoesIdExist<EffectData>(effectRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"effectRef '{effectRef}' of {Id} was not found in database."));
			
			return errorMessages.Count > 0;
		}
	}

	public class ItemConsumableComponent : ItemEquipmentComponent
	{
		public new ItemConsumableComponentData Data
		{
			get
			{
				return (ItemConsumableComponentData)m_Data;
			}
		}

		public override void Init()
		{ }

        public override void OnActivate()
        {
			if (CooldownTimer <= 0)
			{
				Trigger();
				CooldownTimer = 60f / Data.TriggersPerMinute;
			}
		}

        public override void OnDeactivate()
        {
            
        }

		private void Trigger()
        {
			d_OnTrigger?.Invoke(ParentItem, Data.effectRef);
		}

        public override void Tick(float deltaTime)
        {
			CooldownTimer -= deltaTime;
		}
    }
}