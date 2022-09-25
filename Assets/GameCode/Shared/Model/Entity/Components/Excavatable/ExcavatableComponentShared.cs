using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Excavatable
{
	public class ExcavatableComponentShared : FNEComponent
	{
		public ExcavatableComponentData Data
		{
			get
			{
				return (ExcavatableComponentData)base.m_Data;
			}
		}

		protected int hitsRemaining;

		public override void Init()
		{
			hitsRemaining = Data.totalHits;
		}

		public override void Serialize(NetBuffer bw) {
			bw.Write(hitsRemaining);
		}

		public override void Deserialize(NetBuffer br) {
			hitsRemaining = br.ReadInt32();
		}

		public override ushort GetSizeInBytes() { return sizeof(int); }

		public int GetHitsRemaining()
		{
			return hitsRemaining;
		}
	}
}
