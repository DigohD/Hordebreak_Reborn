using FNZ.Shared.Model.Interfaces;
using Lidgren.Network;

namespace FNZ.Shared.Model.World
{

	public class EnvironmentShared : ISerializeable
	{

		public static byte SECONDS_PER_HOUR = 60;
		protected byte m_Hour = 12;
		protected bool freezeTime = false;

		public void Deserialize(NetBuffer reader)
		{
			m_Hour = reader.ReadByte();
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(m_Hour);
		}
	}

}