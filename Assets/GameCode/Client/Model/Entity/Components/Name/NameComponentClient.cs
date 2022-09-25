using FNZ.Shared.Model.Entity.Components.Name;
using Lidgren.Network;

namespace FNZ.Client.Model.Entity.Components.Name
{
	public class NameComponentClient : NameComponentShared
	{
		public override void Serialize(NetBuffer bw)
		{
			base.Serialize(bw);
		}

		public override void Deserialize(NetBuffer br)
		{
			base.Deserialize(br);
		}
	}
}
