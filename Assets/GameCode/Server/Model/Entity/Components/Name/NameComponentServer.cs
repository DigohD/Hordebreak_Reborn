using FNZ.Shared.Model.Entity.Components.Name;
using Lidgren.Network;

namespace FNZ.Server.Model.Entity.Components.Name
{
	public class NameComponentServer : NameComponentShared
	{
		public override void Serialize(NetBuffer bw)
		{
			base.Serialize(bw);

			//Debug.Log("SERVER SERIALIZE: " + entityName);
		}

		public override void Deserialize(NetBuffer br)
		{
			//Debug.Log("SERVER DESERILAIZE BEFORE: " + entityName);
			base.Deserialize(br);

			//Debug.Log("SERVER DESERILAIZE AFTER: " + entityName);
		}
	}
}
