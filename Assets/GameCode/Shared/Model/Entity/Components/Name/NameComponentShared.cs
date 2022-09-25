using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Name
{
	public class NameComponentShared : FNEComponent
	{
		public string entityName = "DEFAULT";

		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(entityName);
		}

		public override void Deserialize(NetBuffer br)
		{
			entityName = br.ReadString();
		}

		public override ushort GetSizeInBytes()
		{
			return (ushort)(entityName.Length * 4);
		}
	}
}
