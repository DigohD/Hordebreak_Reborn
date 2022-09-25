using Lidgren.Network;

namespace FNZ.Shared.Model.Interfaces
{
	public interface ISerializeable
	{
		void Serialize(NetBuffer writer);

		void Deserialize(NetBuffer reader);
	}
}

