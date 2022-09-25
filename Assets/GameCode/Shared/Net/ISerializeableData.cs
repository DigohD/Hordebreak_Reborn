using Lidgren.Network;

namespace FNZ.Shared.Net 
{
	public interface INetSerializeableData
	{
		void NetSerialize(NetBuffer writer);
		void NetDeserialize(NetBuffer reader);
		int GetSizeInBytes();
	}

	public interface ISerializeableData
	{
		void NetSerialize(NetBuffer writer);
		void NetDeserialize(NetBuffer reader);
		void FileSerialize(NetBuffer writer);
		void FileDeserialize(NetBuffer reader);
		int GetSizeInBytes(bool netSerialize);
	}
}