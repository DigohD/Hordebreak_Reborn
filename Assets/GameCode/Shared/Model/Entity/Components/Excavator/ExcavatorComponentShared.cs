using FNZ.Shared.Model.Entity.Components.Excavatable;
using FNZ.Shared.Model.Items;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Excavator
{
	public delegate void OnFuelChange(int newFuel, int previousFuel, int maxFuel);
	public delegate void OnRefuelSlotChange(Item item);

	public class ExcavatorComponentShared : FNEComponent
	{
		public ExcavatorComponentData Data
		{
			get
			{
				return (ExcavatorComponentData)base.m_Data;
			}
		}

		public OnFuelChange d_FuelChange;
		public OnRefuelSlotChange d_RefuelSlotChange;

		protected int m_Fuel;
		public Item RefuelSlot;

        protected FNEEntity m_LockedEntity;
		protected ExcavatableComponentShared m_LockedEntityExcavatable;

        public override void Init()
		{
			m_Fuel = Data.BaseFuel;
		}

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }

		public int GetFuel()
		{
			return m_Fuel;
		}

		public int GetMaximumFuel()
		{
			return Data.BaseFuel;
		}
	}
}
