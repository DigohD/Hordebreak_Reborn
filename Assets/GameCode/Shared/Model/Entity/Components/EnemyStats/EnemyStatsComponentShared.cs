using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.EnemyStats
{
	public class EnemyStatsComponentShared : FNEComponent
	{
		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
